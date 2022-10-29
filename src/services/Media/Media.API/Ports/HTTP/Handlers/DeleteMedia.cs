namespace Media.API.Ports.HTTP.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Core.Interactors;
    using Microsoft.AspNetCore.Http;

    public static class DeleteMedia
    {
        public static async Task<IResult> Handler(DeleteMediaInteractor handler, string id, CancellationToken token)
        {
            var req = new DeleteMediaRequest(id);
            await handler.Handle(req, token);
            return Results.NoContent();
        }
    }
}
