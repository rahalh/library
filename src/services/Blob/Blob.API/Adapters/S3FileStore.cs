namespace Blob.API.Adapters
{
    using System.IO;
    using System.Threading.Tasks;
    using Core;

    public class S3FileStore : IFileStore
    {
        public Task StoreAsync(string key, MemoryStream stream) => throw new System.NotImplementedException();

        public Task RemoveAsync(string key) => throw new System.NotImplementedException();
    }
}
