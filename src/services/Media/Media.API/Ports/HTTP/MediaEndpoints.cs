using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Media.API.Ports.HTTP
{
    using Core;
    using Endpoints;

    public static class MediaEndpoints
    {
        public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/media", async (IMediaService service, CreateRequest req) => await CreateMedia.Handler(service, req));
            endpoints.MapGet("/media/{id}", async (IMediaService service, string id) => await GetMedia.Handler(service, id));
            endpoints.MapDelete("/media/{id}", async (IMediaService service, string id) => await DeleteMedia.Handler(service, id));
            return endpoints;
        }
    }
}
