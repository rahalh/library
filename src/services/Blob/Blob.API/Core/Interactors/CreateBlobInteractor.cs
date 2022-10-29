namespace Blob.API.Core.Interactors
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Configuration;
    using Exceptions;
    using FluentValidation;
    using Helpers;
    using Microsoft.VisualBasic;

    public class CreateBlobInteractor
    {
        private readonly IBlobRepository repo;
        private readonly IFileStore fileStore;
        private readonly S3Settings s3Settings;

        public CreateBlobInteractor(IBlobRepository repo, IFileStore fileStore, S3Settings s3Settings)
        {
            this.repo = repo;
            this.fileStore = fileStore;
            this.s3Settings = s3Settings;
        }

        public async Task<BlobDTO> HandleAsync(CreateBlobRequest request, CancellationToken token)
        {
            var validationResult = new CreateBlobRequest.Validator().Validate(request);
            if (!validationResult.IsValid)
            {
                // todo bad request exception
                throw new EntityValidationException(validationResult.ToDictionary());
            }

            var blob = new Blob(request.Id, request.BlobType, request.Extension, request.Size, this.s3Settings.StorageDomain, this.s3Settings.Prefix);
            // todo use URI instead of string
            await this.fileStore.StoreAsync(blob.Name, request.Content, token);
            // todo resolve storage domain
            await this.repo.SaveAsync(blob, token);
            // todo publish event (blob_uploaded)
            return new BlobDTO(blob.Id, blob.Name, blob.Size, blob.BlobType, blob.Extension, blob.URL, blob.CreateTime, blob.UpdateTime);
        }
    }

    public record CreateBlobRequest(
        string Id,
        long Size,
        string BlobType,
        string Extension,
        Stream Content
    )
    {
        public class Validator : AbstractValidator<CreateBlobRequest>
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
