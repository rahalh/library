namespace Blob.API.Core.Interactors
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Configuration;
    using FluentValidation;
    using Helpers;
    using Microsoft.VisualBasic;
    using Serilog;
    using ValidationException = global::Blob.API.Core.Exceptions.ValidationException;

    public class StoreBlobInteractor
    {
        private readonly IBlobRepository repo;
        private readonly IFileStore fileStore;
        private readonly IEventProducer eventProducer;
        private readonly S3Settings s3Settings;
        private readonly ILogger logger;

        public StoreBlobInteractor(ILogger logger, IBlobRepository repo, IFileStore fileStore, IEventProducer eventProducer, S3Settings s3Settings)
        {
            this.repo = repo;
            this.fileStore = fileStore;
            this.logger = logger
                .ForContext<StoreBlobInteractor>()
                .ForContext("Method", $"{typeof(StoreBlobInteractor).FullName}.{nameof(this.HandleAsync)}");
            this.s3Settings = s3Settings;
            this.eventProducer = eventProducer;
        }

        public async Task<BlobDTO> HandleAsync(StoreBlobRequest request, CancellationToken token)
        {
            var validationResult = new StoreBlobRequest.Validator().Validate(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(JsonSerializer.Serialize(validationResult.ToDictionary()));
            }

            var blob = new Blob(request.Id, request.BlobType, request.Extension, request.Size, this.s3Settings.StorageDomain, this.s3Settings.Prefix);
            await this.fileStore.StoreAsync(blob.Name, request.Content, token);
            try
            {
                await this.repo.SaveAsync(blob, token);
            }
            catch (Exception ex)
            {
                // Rollback persistence
                try { await this.fileStore.RemoveAsync(blob.Id, token); }
                catch (Exception nestedEx) { this.logger.Error(nestedEx, nestedEx.Message); }

                this.logger.Error(ex, ex.Message);
                throw;
            }

            try
            {
                var @event = new Event(DateTime.UtcNow, ProducedEvents.BlobUploaded, new {blob.Id, blob.URL});
                await this.eventProducer.ProduceAsync(@event, token);
                this.logger
                    .ForContext("EventType", ProducedEvents.BlobUploaded)
                    .Information("Event published");
            }
            catch (Exception ex)
            {
                // Rollback persistence
                try { await this.fileStore.RemoveAsync(blob.Id, token); }
                catch (Exception innerEx) { this.logger.Error(innerEx, innerEx.Message); }

                try { await this.fileStore.RemoveAsync(blob.Id, token); }
                catch (Exception innerEx) { this.logger.Error(innerEx, innerEx.Message); }

                this.logger
                    .ForContext("EventType", ProducedEvents.BlobUploaded)
                    .Error(ex, "Failed to publish the event");
                throw;
            }

            return new BlobDTO(blob.Id, blob.Name, blob.Size, blob.BlobType, blob.Extension, blob.URL, blob.CreateTime, blob.UpdateTime);
        }
    }

    public record StoreBlobRequest(
        string Id,
        long Size,
        string BlobType,
        string Extension,
        Stream Content
    )
    {
        public class Validator : AbstractValidator<StoreBlobRequest>
        {
            public Validator()
            {
                this.RuleFor(x => x.Id)
                    .NotNull()
                    .MaximumLength(30)
                    .MinimumLength(1);

                this.RuleFor(x => x.BlobType)
                    .NotNull()
                    .NotEmpty()
                    .Must(x => EnumUtils.TryParseWithMemberName<BlobTypes>(x, out _))
                    .WithErrorCode("unsupported blob type")
                    .When(x => !string.IsNullOrEmpty(x.BlobType));

                this.RuleFor(x => x.Extension)
                    .NotNull()
                    .MinimumLength(1)
                    .MaximumLength(8)
                    .Equal("pdf", StringComparer.OrdinalIgnoreCase)
                    .When(x => EnumUtils.GetEnumValueOrDefault<BlobTypes>(x.BlobType) == BlobTypes.Application)
                    .Must(x => new[] {"mpeg", "vorbis", "aac", "mp3", "ogg"}.Contains(Strings.LCase(x)))
                    .When(x => EnumUtils.GetEnumValueOrDefault<BlobTypes>(x.BlobType) == BlobTypes.Audio)
                    .Must(x => new[] {"h264", "mp4", "mpeg", "ogg"}.Contains(Strings.LCase(x)))
                    .When(x => EnumUtils.GetEnumValueOrDefault<BlobTypes>(x.BlobType) == BlobTypes.Video);

                this.RuleFor(x => x.Size)
                    .NotNull()
                    // max 25 MB
                    .LessThan(25 * (long)Math.Pow(1024, 2))
                    .When(x => EnumUtils.GetEnumValueOrDefault<BlobTypes>(x.BlobType) == BlobTypes.Application)
                    // max 256 MB
                    .LessThan(256 * (long)Math.Pow(1024, 2))
                    .When(x => EnumUtils.GetEnumValueOrDefault<BlobTypes>(x.BlobType) == BlobTypes.Audio)
                    // max 8 GB
                    .LessThan(8192 * (long)Math.Pow(1024, 2))
                    .When(x => EnumUtils.GetEnumValueOrDefault<BlobTypes>(x.BlobType) == BlobTypes.Video);
            }
        }
    };
}
