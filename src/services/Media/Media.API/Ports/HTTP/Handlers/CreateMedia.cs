namespace Media.API.Ports.HTTP.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Core.Interactors;
    using Microsoft.AspNetCore.Http;

    public static class CreateMedia
    {
        public static async Task<IResult> Handler(CreateMediaInteractor handler, CreateMediaRequest req,
            CancellationToken token)
        {
            var media = await handler.Handle(req, token);
            return Results.CreatedAtRoute("GetMedia", new {id = media.ExternalId}, media);
        }
    }
}
