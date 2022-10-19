namespace Media.API.Core
{
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

        // TODO update total views
        public async Task<Media> Get(string id, CancellationToken token) => await this.repo.FetchByID(id, token);

        public async Task Delete(string id, CancellationToken token) => await this.repo.Remove(id, token);
    }
}
