namespace Media.Infrastructure.Transport.HTTP.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Interactors;
    using Microsoft.AspNetCore.Http;

    public static class CreateMedia
    {
        public static async Task<IResult> Handler(CreateMediaInteractor handler, CreateMediaRequest req,
            CancellationToken token)
        {
            var media = await handler.HandleAsync(req, token);
            return Results.CreatedAtRoute("GetMedia", new {id = media.ExternalId}, media);
        }
    }
}
