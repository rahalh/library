namespace Blob.API.Core
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IFileStore
    {
        public Task StoreAsync(string key, MemoryStream stream);
        public Task RemoveAsync(string key);
    }
}
