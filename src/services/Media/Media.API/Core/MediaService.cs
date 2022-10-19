namespace Media.API.Core
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Dapper;

    public record ListMediaResult(List<Media> Medias, string NextPageToken);

    public class MediaService : IMediaService
    {
        private readonly IMediaRepository repo;

        public MediaService(IMediaRepository repo) => this.repo = repo;

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

        public async Task<Media> Get(string id, CancellationToken token)
        {
            var media = await this.repo.FetchByID(id, token);
            if (media is not null)
            {
                await this.repo.IncrementViewCount(id, token);
                media.TotalViews++;
            }
            return media;
        }

        public async Task Delete(string id, CancellationToken token) => await this.repo.Remove(id, token);

        public async Task<ListMediaResult> List(PaginationParams parameters, CancellationToken token)
        {
            parameters.Size++;
            var medias = await this.repo.List(parameters, token);
            string nextToken = null;
            if (medias.Count > parameters.Size)
            {
                nextToken = medias.LastOrDefault()?.ExternalID;
                medias = medias.SkipLast(1).AsList();
            }

            return new ListMediaResult(medias, nextToken);
        }
    }
}
