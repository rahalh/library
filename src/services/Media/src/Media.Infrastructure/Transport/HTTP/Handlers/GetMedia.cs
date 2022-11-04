namespace Media.Infrastructure.Transport.HTTP.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Interactors;
    using Microsoft.AspNetCore.Http;

    public static class GetMedia
    {
        public static async Task<IResult> Handler(GetMediaInteractor handler, string id, CancellationToken token)
        {
            var req = new GetMediaRequest(id);
            var media = await handler.HandleAsync(req, token);
            return Results.Ok(media);
        }
    }
}
