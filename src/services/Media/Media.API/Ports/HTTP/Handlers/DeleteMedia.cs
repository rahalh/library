namespace Media.API.Ports.HTTP.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Media.API.Core;
    using Microsoft.AspNetCore.Http;

    public static class DeleteMedia
    {
        public static async Task<IResult> Handler(IMediaService srv, string id, CancellationToken token)
        {
            await srv.Delete(id, token);
            return Results.NoContent();
        }
    }
}
