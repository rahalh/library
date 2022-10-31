namespace Media.API.Core.Interactors
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;
    using Serilog;

    public class DeleteMediaInteractor
    {
        private readonly IMediaRepository repo;
        private readonly IEventProducer eventProducer;
        private readonly ILogger logger;

        public DeleteMediaInteractor(ILogger logger, IMediaRepository repo, IEventProducer eventProducer)
        {
            this.repo = repo;
            this.eventProducer = eventProducer;
            this.logger = logger
                .ForContext<DeleteMediaInteractor>()
                .ForContext("Method", $"{typeof(DeleteMediaInteractor).FullName}.{nameof(this.HandleAsync)}");
        }

        public async Task HandleAsync(DeleteMediaRequest request, CancellationToken token)
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            await this.repo.Remove(request.Id, token);
            try
            {
                var @event = new Event(DateTime.UtcNow, ProducedEvents.MediaRemoved, "Media", request.Id);
                await this.eventProducer.ProduceAsync(@event, token);

                this.logger
                    .ForContext("EventType", ProducedEvents.MediaRemoved)
                    .Information("Event published");
                transactionScope.Complete();
            }
            catch (Exception ex)
            {
                this.logger
                    .ForContext("EventType", ProducedEvents.MediaRemoved)
                    .Error(ex, "Failed to publish the event");
                throw;
            }
        }
    }

    public record DeleteMediaRequest(string Id);
}
