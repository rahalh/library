namespace Blob.API.Ports.HTTP.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Core;
    using Microsoft.AspNetCore.Http;

    public static class DeleteBlob
    {
        public static async Task<IResult> Handle(IBlobService srv, string id, CancellationToken token)
        {
            await srv.DeleteAsync(id, token);
            return Results.NoContent();
        }
    }
}
