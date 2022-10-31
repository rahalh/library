namespace Media.API.Core.Interactors
{
    using System;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Exceptions;
    using FluentValidation;
    using Serilog;
    using ValidationException = FluentValidation.ValidationException;

    public class SetContentURLInteractor
    {
        private readonly IMediaRepository repo;
        private readonly IEventProducer eventProducer;
        private readonly ILogger logger;

        public SetContentURLInteractor(ILogger logger, IMediaRepository repo, IEventProducer eventProducer)
        {
            this.repo = repo;
            this.eventProducer = eventProducer;
            this.logger = logger
                .ForContext<SetContentURLInteractor>()
                .ForContext("Method", $"{typeof(SetContentURLInteractor).FullName}.{nameof(this.HandleAsync)}");
        }

        public async Task HandleAsync(string message, CancellationToken token)
        {
            try
            {
                var request = JsonSerializer.Deserialize<SetContentURLRequest>(message);
                var validationResults = new SetContentURLRequest.Validator().Validate(request);
                if (!validationResults.IsValid)
                {
                    throw new ValidationException(JsonSerializer.Serialize(validationResults.ToDictionary()));
                }

                var exists = await this.repo.CheckExists(request.Id, token);
                if (!exists)
                {
                    throw new NotFoundException($"Can't find media with Id: {request.Id}");
                }

                await this.repo.SetContentURL(request.Id, request.URL, token);
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, ex.Message);
                try
                {
                    var @event = new Event(DateTime.UtcNow, ProducedEvents.MediaUpdateFailed, "Media", message);
                    await this.eventProducer.ProduceAsync(@event, token);
                }
                catch (Exception innerEx)
                {
                    this.logger
                        .ForContext("EventType", ProducedEvents.MediaUpdateFailed)
                        .Error(innerEx, "Failed to publish the event");
                }
                throw;
            }
        }
    }

    public record SetContentURLRequest(string Id, string URL)
    {
        public class Validator : AbstractValidator<SetContentURLRequest>
        {
            public Validator()
            {
                this.RuleFor(x => x.URL)
                    .NotNull()
                    .NotEmpty()
                    .MaximumLength(2048);

                this.RuleFor(x => x.Id)
                    .NotNull()
                    .NotEmpty();
            }
        }
    };
}
