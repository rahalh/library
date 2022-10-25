namespace Blob.API.Core
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Exceptions;

    public class BlobService : IBlobService
    {
        private readonly IBlobRepository repo;
        private readonly IFileStore fileStore;

        public BlobService(IBlobRepository repo, IFileStore fileStore)
        {
            this.repo = repo;
            this.fileStore = fileStore;
        }

        public async Task<Blob> GetByIdAsync(string id, CancellationToken token) => await this.repo.GetByIdAsync(id, token);

        public async Task<Blob> SaveAsync(Blob blob, Stream stream, CancellationToken token)
        {
            var validationResult = new BlobValidator().Validate(blob);
            if (!validationResult.IsValid)
            {
                throw new EntityValidationException(validationResult.ToDictionary());
            }

            await this.fileStore.StoreAsync(blob.Name, stream, token);
            await this.repo.SaveAsync(blob ,token);
            // todo publish event (blob_uploaded)
            return blob;
        }

        public async Task DeleteAsync(string id, CancellationToken token)
        {
            await this.fileStore.RemoveAsync(id, token);
            await this.repo.RemoveAsync(id, token);
            // todo send this event to the media service
        }
    }
}
