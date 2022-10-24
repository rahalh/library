namespace Media.API.Ports.HTTP.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Media.API.Adapters.Exceptions;
    using Media.API.Core;
    using Microsoft.AspNetCore.Http;

    public static class GetMedia
    {
        public static async Task<IResult> Handler(IMediaService srv, string id, CancellationToken token)
        {
            try
            {
                var media = await srv.GetById(id, token);
                return Results.Ok(media);
            }
            catch (NotFoundException _)
            {
                return Results.NotFound();
            }
        }
    }
}
