namespace Media.Infrastructure.Transport.HTTP.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Interactors;
    using Microsoft.AspNetCore.Http;

    public static class DeleteMedia
    {
        public static async Task<IResult> Handler(DeleteMediaInteractor handler, string id, CancellationToken token)
        {
            var req = new DeleteMediaRequest(id);
            await handler.HandleAsync(req, token);
            return Results.NoContent();
        }
    }
}
