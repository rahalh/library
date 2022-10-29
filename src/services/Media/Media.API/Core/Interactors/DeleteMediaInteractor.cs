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
        private readonly IMediaEventBus eventBus;
        private readonly ILogger logger;

        public DeleteMediaInteractor(ILogger logger, IMediaRepository repo, IMediaEventBus eventBus)
        {
            this.repo = repo;
            this.eventBus = eventBus;
            this.logger = logger.ForContext<DeleteMediaInteractor>();
        }

        public async Task Handle(DeleteMediaRequest request, CancellationToken token)
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            await this.repo.Remove(request.Id, token);
            try
            {
                await this.eventBus.PublishAsync(ProducedEventType.MediaRemoved, request.Id);
                this.logger
                    .ForContext("eventType", ProducedEventType.MediaRemoved)
                    .Information("Event published");
                transactionScope.Complete();
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, ex.Message);
                this.logger
                    .ForContext("eventType", ProducedEventType.MediaRemoved)
                    .Information("Failed to publish the event");
                throw;
            }
        }
    }

    public record DeleteMediaRequest(string Id);
}
