namespace Media.API.Ports.HTTP.Handlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Adapters.Exceptions;
    using Core;
    using Core.Exceptions;
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
                var author = await srv.Create(new Media(req.Title, req.Description, req.PublishDate, req.MediaType), token);
                return Results.Ok(author);
            }
            catch (EntityValidationException ex)
            {
                return Results.ValidationProblem(ex.Errors);
            }
            catch (EntityExistsException ex)
            {
                return Results.Conflict();
            }
        }
    }
}
