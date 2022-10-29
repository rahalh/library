namespace Blob.API.Ports.HTTP.Handlers
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Core.Interactors;
    using Microsoft.AspNetCore.Http;
    using Microsoft.VisualBasic;

    public static class SaveBlob
    {
        public static async Task<IResult> Handler(CreateBlobInteractor handler, HttpRequest req,
            CancellationToken token)
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

            await using var stream = file.OpenReadStream();
            await handler.HandleAsync(new CreateBlobRequest(id, size, blobType, extension, stream), token);
            return Results.NoContent();
        }
    }
}
