namespace Media.API.Ports.HTTP.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Core.Interactors;
    using Microsoft.AspNetCore.Http;

    public static class ListMedia
    {
        public static async Task<IResult> Handler(ListMediaInteractor handler, int? pageSize, string pageToken, CancellationToken token)
        {
            var res = await handler.Handle(new ListMediaRequest(pageSize ?? 0, pageToken), token);
            return Results.Ok(res);
        }
    }
}
