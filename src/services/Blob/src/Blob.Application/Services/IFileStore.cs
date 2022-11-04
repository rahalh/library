namespace Blob.Application.Services
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IFileStore
    {
        public Task StoreAsync(string key, Stream stream, CancellationToken token);
        public Task RemoveAsync(string key, CancellationToken token);
    }
}
