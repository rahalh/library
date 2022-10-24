namespace Media.API.Core
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IMediaService
    {
        public Task<Media> Create(Media media, CancellationToken token);
        public Task<Media> GetById(string id, CancellationToken token);
        public Task Delete(string id, CancellationToken token);
        public Task<ListMediaResult> List(PaginationParams parameters, CancellationToken token);
        public Task SetContentURL(string id, string url, CancellationToken token);
    }
}
