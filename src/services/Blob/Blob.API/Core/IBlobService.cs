namespace Blob.API.Core
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IBlobService
    {
        public Task<Blob> GetByIdAsync(string id, CancellationToken token);
        public Task<Blob> SaveAsync(Blob blob, Stream stream, CancellationToken token);
        public Task DeleteAsync(string id, CancellationToken token);
    }
}
