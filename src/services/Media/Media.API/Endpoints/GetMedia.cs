using System.Threading.Tasks;
using Media.API.Core;
using Microsoft.AspNetCore.Http;

public static class GetMedia
{
    public static async Task<IResult> Handler(IMediaService srv, string id)
    {
        try
        {
            var media = await srv.Get(id);
            return Results.Ok(media);
        }
        catch (NotFoundException _)
        {
            return Results.NotFound();
        }
    }
}
