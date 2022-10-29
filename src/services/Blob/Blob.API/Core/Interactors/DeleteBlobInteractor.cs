namespace Blob.API.Core.Interactors
{
    using System.Threading;
    using System.Threading.Tasks;

    public class DeleteBlobInteractor
    {
        private readonly IBlobRepository repo;
        private readonly IFileStore fileStore;

        public DeleteBlobInteractor(IBlobRepository repo, IFileStore fileStore)
        {
            this.repo = repo;
            this.fileStore = fileStore;
        }

        public async Task HandleAsync(DeleteBlobRequest req, CancellationToken token)
        {
            await this.fileStore.RemoveAsync(req.Id, token);
            await this.repo.RemoveAsync(req.Id, token);
            // todo send this event to the media service
        }
    }

    public record DeleteBlobRequest(string Id);
}
