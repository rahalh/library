namespace Media.API.Core
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

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

        public async Task<List<Media>> List(PaginationParams parameters, CancellationToken token)
        {
            parameters.Size++;
            var medias = await this.repo.List(parameters, token);
            return medias;
        }
    }
}
