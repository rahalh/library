namespace Blob.Infrastructure.Transport.HTTP.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Interactors;
    using Microsoft.AspNetCore.Http;

    public static class GetBlob
    {
        public static async Task<IResult> Handler(GetBlobByIdInteractor handler, string id, CancellationToken token)
        {
            var res = await handler.HandleAsync(new GetBlobByIdRequest(id), token);
            return Results.Ok(res);
        }
    }
}
