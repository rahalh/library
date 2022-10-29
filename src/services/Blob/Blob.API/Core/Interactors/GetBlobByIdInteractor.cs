namespace Blob.API.Core.Interactors
{
    using System.Threading;
    using System.Threading.Tasks;

    public class GetBlobByIdInteractor
    {
        private readonly IBlobRepository repo;

        public GetBlobByIdInteractor(IBlobRepository repo) => this.repo = repo;

        public async Task<BlobDTO> HandleAsync(GetBlobByIdRequest req, CancellationToken token)
        {
            var blob = await this.repo.GetByIdAsync(req.Id, token);
            return new BlobDTO(blob.Id, blob.Name, blob.Size, blob.BlobType, blob.Extension, blob.URL, blob.CreateTime, blob.UpdateTime);
        }
    }

    public record GetBlobByIdRequest(string Id);
}
