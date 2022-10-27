namespace Media.API.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;
    using Adapters.Exceptions;
    using Dapper;
    using Exceptions;
    using Serilog;

    public record ListMediaResult(List<Media> Medias, string NextPageToken);

    public class MediaService : IMediaService
    {
        private readonly IMediaRepository repo;
        private readonly IMediaEventBus eventBus;
        private readonly ILogger logger;

        public MediaService(ILogger logger, IMediaRepository repo, IMediaEventBus eventBus)
        {
            this.repo = repo;
            this.eventBus = eventBus;
            this.logger = logger.ForContext<MediaService>();
        }

        public async Task<Media> Create(Media media, CancellationToken token)
        {
            var validationResult = new MediaValidator().Validate(media);
            if (!validationResult.IsValid)
            {
                throw new EntityValidationException(validationResult.ToDictionary());
            }

            await this.repo.Save(media, token);
            return media;
        }

        public async Task<Media> GetById(string id, CancellationToken token)
        {
            var media = await this.repo.FetchById(id, token);
            if (media is null)
            {
                throw new NotFoundException();
            }
            media.TotalViews += 1;
            await this.repo.SetViewCount(id, media.TotalViews, token);
            return media;
        }

        public async Task Delete(string id, CancellationToken token)
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            await this.repo.Remove(id, token);
            try
            {
                await this.eventBus.PublishAsync(ProducedEventType.MediaRemoved, id);
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

        public async Task<ListMediaResult> List(PaginationParams parameters, CancellationToken token)
        {
            var medias = await this.repo.List(new PaginationParams(parameters.Token, parameters.Size + 1), token);
            string nextToken = null;
            if (medias.Count > parameters.Size)
            {
                nextToken = medias.LastOrDefault()?.ExternalId;
                medias = medias.SkipLast(1).AsList();
            }

            return new ListMediaResult(medias, nextToken);
        }

        public async Task SetContentURL(string id, string url, CancellationToken token) => await this.repo.SetContentURL(id, url, token);
    }
}
