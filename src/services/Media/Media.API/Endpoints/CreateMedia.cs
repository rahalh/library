namespace Media.API.Endpoints
{
    using System;
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
        public static async Task<IResult> Handler(IMediaService srv, CreateRequest req)
        {
            try
            {
                var author = await srv.Create(new Media
                {
                    Title = req.Title,
                    Description = req.Description,
                    PublishDate = req.PublishDate
                });
                return Results.Ok(author);
            }
            catch (EntityValidationException ex)
            {
                return Results.BadRequest(new {ex.errors});
            }
        }
    }
}
