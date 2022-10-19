namespace Media.API.Ports.HTTP.Endpoints
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Core;
    using Microsoft.AspNetCore.Http;

    public record ListRequest(
        string PageToken,
        int PageSize
    );

    public record ListResponse(
        List<Media> Medias,
        string NextPageToken
    );

    public static class ListMedia
    {
        public static async Task<IResult> Handler(IMediaService srv, ListRequest req, CancellationToken token)
        {
            var res = await srv.List(new PaginationParams(req.PageToken, req.PageSize), token);
            return Results.Ok(new ListResponse(res.Medias, res.NextPageToken));
        }
    }
}
