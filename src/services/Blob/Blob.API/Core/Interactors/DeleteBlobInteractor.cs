namespace Blob.API.Core.Interactors
{
    using System;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Exceptions;
    using FluentValidation;
    using Serilog;
    using ValidationException = global::Blob.API.Core.Exceptions.ValidationException;

    public class DeleteBlobInteractor
    {
        private readonly IBlobRepository repo;
        private readonly IFileStore fileStore;
        private readonly IEventProducer eventProducer;
        private readonly ILogger logger;

        public DeleteBlobInteractor(ILogger logger, IBlobRepository repo, IFileStore fileStore, IEventProducer eventProducer)
        {
            this.repo = repo;
            this.fileStore = fileStore;
            this.eventProducer = eventProducer;
            this.logger = logger.ForContext<DeleteBlobInteractor>().ForContext("Method", $"{typeof(DeleteBlobInteractor).FullName}.{nameof(this.HandleAsync)}");
        }

        public async Task HandleAsync(DeleteBlobRequest req, CancellationToken token)
        {
            var validationResult = new DeleteBlobRequest.Validator().Validate(req);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(JsonSerializer.Serialize(validationResult.ToDictionary()));
            }

            var blob = await this.repo.GetByIdAsync(req.Id, token);
            if (blob is null)
            {
                throw new NotFoundException($"Can't find Blob with Id: {req.Id}");
            }

            var key = blob.Name;
            await this.fileStore.RemoveAsync(key, token);

            await this.repo.RemoveAsync(req.Id, token);

            try
            {
                var @event = new Event(DateTime.UtcNow, ProducedEvents.BlobRemoved, new {req.Id});
                await this.eventProducer.ProduceAsync(@event, token);

                this.logger
                    .ForContext("EventType", ProducedEvents.BlobRemoved)
                    .Information("Event published");
            }
            catch (Exception ex)
            {
                this.logger
                    .ForContext("EventType", ProducedEvents.BlobRemoved)
                    .Error(ex, "Failed to publish the event");
                throw;
            }
        }
    }

    public record DeleteBlobRequest(string Id)
    {
        public class Validator : AbstractValidator<DeleteBlobRequest>
        {
            public Validator() =>
                this.RuleFor(x => x.Id)
                    .NotNull()
                    .NotEmpty()
                    .MaximumLength(30);
        }
    };
}
