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

            // todo save to repo
            // todo save to file store
            return blob;
        }

        public async Task DeleteAsync(string id, CancellationToken token)
        {
            // todo delete from repo
            // todo delete from file store as well
            // todo send this event to the media service
        }
    }
}
