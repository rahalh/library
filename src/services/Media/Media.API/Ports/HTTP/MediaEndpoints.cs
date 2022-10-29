namespace Media.API.Ports.HTTP
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Routing;
    using Handlers;

    public static class MediaEndpoints
    {
        public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/api/media", CreateMedia.Handler);
            endpoints.MapGet("/api/media/{id}", GetMedia.Handler).WithName("GetMedia");
            endpoints.MapDelete("/api/media/{id}", DeleteMedia.Handler);
            endpoints.MapGet("/api/media", ListMedia.Handler);

            return endpoints;
        }
    }
}
