namespace Blob.Infrastructure.Transport.HTTP
{
    using Handlers;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Routing;

    public static class BlobEndpoints
    {
        public static IEndpointRouteBuilder MapBlobEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/api/blobs/media/{id:alpha}", SaveBlob.Handler);
            endpoints.MapDelete("/api/blobs/media/{id:alpha}", DeleteBlob.Handler);
            endpoints.MapGet("/api/blobs/media/{id:alpha}", GetBlob.Handler);

            return endpoints;
        }
    }
}
