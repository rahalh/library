namespace Blob.API.Ports.HTTP
{
    using System.Threading;
    using Core;
    using Handlers;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Configuration;

    public static class BlobEndpoints
    {
        public static IEndpointRouteBuilder MapBlobEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/api/blobs/media/{id}",
                async (IConfiguration configuration, [FromServices] IBlobService service, HttpRequest req,
                    CancellationToken token) => await SaveBlob.Handle(configuration, service, req, token));
            endpoints.MapDelete("/api/blobs/media/{id}",
                async (IBlobService service, string id, CancellationToken token) =>
                    await DeleteBlob.Handle(service, id, token));
            endpoints.MapGet("/api/blobs/media/{id}",
                async (IBlobService service, string id, CancellationToken token) =>
                    await GetBlob.Handle(service, id, token));

            return endpoints;
        }
    }
}
