namespace Media.API.Ports.HTTP
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Routing;
    using Handlers;

    public static class MediaEndpoints
    {
        public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/media", CreateMedia.Handler);
            endpoints.MapGet("/media/{id}", GetMedia.Handler).WithName("GetMedia");
            endpoints.MapDelete("/media/{id}", DeleteMedia.Handler);
            endpoints.MapGet("/media", ListMedia.Handler);

            return endpoints;
        }
    }
}
