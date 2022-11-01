namespace Blob.API.Transport.HTTP
{
    using Handlers;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Routing;

    public static class BlobEndpoints
    {
        public static IEndpointRouteBuilder MapBlobEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/api/blobs/media/{id}", SaveBlob.Handler);
            endpoints.MapDelete("/api/blobs/media/{id}", DeleteBlob.Handler);
            endpoints.MapGet("/api/blobs/media/{id}", GetBlob.Handler);

            return endpoints;
        }
    }
}
