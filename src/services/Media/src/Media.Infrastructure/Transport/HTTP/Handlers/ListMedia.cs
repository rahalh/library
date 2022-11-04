namespace Media.Infrastructure.Transport.HTTP.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Interactors;
    using Microsoft.AspNetCore.Http;

    public static class ListMedia
    {
        // todo dotnet checks whether a param is required (nullable) or not not, throws an error when param is missing, test this
        // this is not true for class / model binding, if a value isn't present is http body then gets default value
        public static async Task<IResult> Handler(ListMediaInteractor handler, int? pageSize, string? pageToken, CancellationToken token)
        {
            var res = await handler.HandleAsync(new ListMediaRequest(pageSize ?? default, pageToken), token);
            return Results.Ok(res);
        }
    }
}
