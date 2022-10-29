namespace Blob.API.Ports.HTTP.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Core.Interactors;
    using Microsoft.AspNetCore.Http;

    public static class DeleteBlob
    {
        public static async Task<IResult> Handler(DeleteBlobInteractor interactor, string id, CancellationToken token)
        {
            await interactor.HandleAsync(new DeleteBlobRequest(id), token);
            return Results.NoContent();
        }
    }
}
