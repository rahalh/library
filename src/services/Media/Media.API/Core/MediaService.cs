using System.Threading.Tasks;

namespace Media.API.Core
{
    public class MediaService : IMediaService
    {
        private readonly IMediaRepository repo;

        public MediaService(IMediaRepository repo) => this.repo = repo;

        // TODO use cancellation token
        public async Task<Media> Create(Media media)
        {
            var validationResult = new MediaValidator().Validate(media);
            if (!validationResult.IsValid)
            {
                throw new EntityValidationException(validationResult.ToDictionary());
            }
            await this.repo.Save(media);
            return media;
        }

        // TODO update total views
        public async Task<Media> Get(string id) => await this.repo.FetchByID(id);

        public async Task Delete(string id) => await this.repo.Remove(id);
    }
}
