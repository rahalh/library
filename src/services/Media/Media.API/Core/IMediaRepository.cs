namespace Media.API.Core
{
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    public interface IMediaRepository
    {
        public Task Save(Media media, CancellationToken token);
        public Task<Media> FetchByID(string id, CancellationToken token);
        public Task Remove(string id, CancellationToken token);
        public Task<List<Media>> List(PaginationParams parameters, CancellationToken token);
        public Task IncrementViewCount(string id, CancellationToken token);
    }
}
