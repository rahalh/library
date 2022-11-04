namespace Blob.Infrastructure.Transport.HTTP.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Interactors;
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
