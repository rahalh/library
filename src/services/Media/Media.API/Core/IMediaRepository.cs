namespace Media.API.Core
{
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    public interface IMediaRepository
    {
        public Task SaveAsync(Media media, CancellationToken token);
        public Task<Media> FetchByIdAsync(string id, CancellationToken token);
        public Task RemoveAsync(string id, CancellationToken token);
        public Task<List<Media>> ListAsync(PaginationParams parameters, CancellationToken token);
        public Task SetViewCountAsync(string id, int count, CancellationToken token);
        public Task SetContentURLAsync(string id, string url, CancellationToken token);
        public Task<bool> CheckExistsAsync(string id, CancellationToken token);
    }
}
