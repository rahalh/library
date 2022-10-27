namespace Media.API.Ports.HTTP.Handlers
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Core;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    public record ListRequest(
        string PageToken,
        int? PageSize
    );

    public record ListResponse(
        List<Media> Items,
        string NextPageToken
    );

    public static class ListMedia
    {
        public static async Task<IResult> Handler(IMediaService srv, ListRequest req, CancellationToken token)
        {
            var res = await srv.List(new PaginationParams(req?.PageToken, req?.PageSize), token);
            return Results.Ok(new ListResponse(res.Medias, res.NextPageToken));
        }
    }
}
