namespace Blob.API.Ports.HTTP.Handlers
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Core;
    using Core.Exceptions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.VisualBasic;

    public static class SaveBlob
    {
        public static async Task<IResult> Handle(IConfiguration config, IBlobService srv, HttpRequest req, CancellationToken token)
        {
            var id = req.RouteValues.GetValueOrDefault("id") as string;
            var file = req.Form.Files[0];
            var contentType = Strings.Split(file.ContentType, "/");
            if (contentType.Length < 2)
            {
                return Results.BadRequest();
            }
            var blobType = contentType[0];
            var extension = contentType[1];
            var size = file.Length;
            var storageDomain = config["AWS:S3:StorageDomain"];
            var storagePath = config["AWS:S3:Prefix"];

            var stream = file.OpenReadStream();
            try
            {
                await srv.SaveAsync(new Blob(id, blobType, extension, size, storageDomain, storagePath), stream, token);
                return Results.NoContent();
            }
            catch (EntityValidationException ex)
            {
                return Results.ValidationProblem(ex.Errors);
            }
        }
    }
}
