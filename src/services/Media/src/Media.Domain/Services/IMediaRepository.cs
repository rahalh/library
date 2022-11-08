namespace Media.Domain.Services
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IMediaRepository
    {
        public Task SaveAsync(Media media, CancellationToken token);
        public Task<Media?> FetchByIdAsync(string id, CancellationToken token);
        public Task RemoveAsync(string id, CancellationToken token);
        public Task<IReadOnlyList<Media>> ListAsync(int pageSize, string? pageToken, CancellationToken token);
        public Task SetViewCountAsync(string id, int count, CancellationToken token);
        public Task SetContentURLAsync(string id, string url, CancellationToken token);
        public Task<bool> CheckExistsAsync(string id, CancellationToken token);
    }
}
