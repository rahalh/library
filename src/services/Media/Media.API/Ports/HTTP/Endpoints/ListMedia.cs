namespace Media.API.Ports.HTTP.Endpoints
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Core;
    using Dapper;
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
            var medias = await srv.List(new PaginationParams(req.PageToken, req.PageSize), token);

            string nextToken = null;
            if (medias.Count > req.PageSize)
            {
                nextToken = medias.LastOrDefault()?.ExternalID;
                medias = medias.SkipLast(1).AsList();
            }
            return Results.Ok(new ListResponse(medias, nextToken));
        }
    }
}
