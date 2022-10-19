using System.Threading;
using System.Threading.Tasks;

namespace Media.API.Core
{
    public interface IMediaRepository
    {
        public Task Save(Media media, CancellationToken cancellationToken = default);
        public Task<Media> FetchByID(string id, CancellationToken cancellationToken = default);
        public Task Remove(string id, CancellationToken cancellationToken = default);
    }
}
