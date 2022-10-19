namespace Media.API.Ports.HTTP.Endpoints
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Core;
    using Microsoft.AspNetCore.Http;

    public record CreateRequest(
        string Title,
        string Description,
        string LanguageCode,
        string MediaType,
        DateTime PublishDate
    );

    public static class CreateMedia
    {
        public static async Task<IResult> Handler(IMediaService srv, CreateRequest req, CancellationToken token)
        {
            try
            {
                var author = await srv.Create(new Media
                {
                    Title = req.Title,
                    Description = req.Description,
                    PublishDate = req.PublishDate
                }, token);
                return Results.Ok(author);
            }
            catch (EntityValidationException ex)
            {
                return Results.BadRequest(new {ex.errors});
            }
        }
    }
}
