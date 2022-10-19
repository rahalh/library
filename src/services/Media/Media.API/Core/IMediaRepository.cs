using System.Threading;
using System.Threading.Tasks;

namespace Media.API.Core
{
    public interface IMediaRepository
    {
        public Task Save(Media media, CancellationToken token);
        public Task<Media> FetchByID(string id, CancellationToken token);
        public Task Remove(string id, CancellationToken token);
    }
}
