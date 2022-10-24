namespace Blob.API.Adapters
{
    using System.Threading;
    using System.Threading.Tasks;
    using Core;

    public class DDBRepository : IBlobRepository
    {
        public Task<Blob> SaveAsync(Blob blob, CancellationToken token) => throw new System.NotImplementedException();

        public Task RemoveAsync(string id, CancellationToken token) => throw new System.NotImplementedException();

        public Task<Blob> GetByIdAsync(string id, CancellationToken token) => throw new System.NotImplementedException();
    }
}
