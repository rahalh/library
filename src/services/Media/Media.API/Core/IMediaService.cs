using System.Threading.Tasks;

namespace Media.API.Core
{
    public interface IMediaService
    {
        public Task<Media> Create(Media media);
        public Task<Media> Get(string id);
        public Task Delete(string id);
    }
}
