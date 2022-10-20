namespace Media.API.Ports.HTTP
{
    using System.Threading;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Routing;
    using Core;
    using Endpoints;
    using Microsoft.AspNetCore.Mvc;

    public static class MediaEndpoints
    {
        public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/media", async (IMediaService service, CreateRequest req, CancellationToken token) => await CreateMedia.Handler(service, req, token));

            endpoints.MapGet("/media/{id}", async (IMediaService service, string id, CancellationToken token) => await GetMedia.Handler(service, id, token));

            endpoints.MapDelete("/media/{id}", async (IMediaService service, string id, CancellationToken token) => await DeleteMedia.Handler(service, id, token));

            endpoints.MapGet("/media", async ([FromServices] IMediaService service, [FromBody] ListRequest req, CancellationToken token) => await ListMedia.Handler(service, req, token));

            return endpoints;
        }
    }
}
