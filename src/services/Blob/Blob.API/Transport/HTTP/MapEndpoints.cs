namespace Blob.API.Transport.HTTP
{
    using Handlers;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Routing;

    public static class BlobEndpoints
    {
        public static IEndpointRouteBuilder MapBlobEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/api/blobs/media/{id:string}", SaveBlob.Handler);
            endpoints.MapDelete("/api/blobs/media/{id:string}", DeleteBlob.Handler);
            endpoints.MapGet("/api/blobs/media/{id:string}", GetBlob.Handler);

            return endpoints;
        }
    }
}
