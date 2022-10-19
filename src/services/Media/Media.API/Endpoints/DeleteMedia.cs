using System.Threading.Tasks;
using Media.API.Core;
using Microsoft.AspNetCore.Http;

public static class DeleteMedia
{
    public static async Task<IResult> Handler(IMediaService srv, string id)
    {
        await srv.Delete(id);
        return Results.NoContent();
    }
}
