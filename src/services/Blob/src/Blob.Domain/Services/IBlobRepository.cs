namespace Blob.Domain.Services
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IBlobRepository
    {
        public Task<Blob> SaveAsync(Blob blob, CancellationToken token);
        public Task RemoveAsync(string id, CancellationToken token);
        public Task<Blob> GetByIdAsync(string id, CancellationToken token);
    }
}
