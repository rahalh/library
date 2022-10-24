namespace Blob.API.Ports.HTTP.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Adapters.Exceptions;
    using Core;
    using Microsoft.AspNetCore.Http;

    public static class GetBlob
    {
        public static async Task<IResult> Handle(IBlobService srv, string id, CancellationToken token)
        {
            try
            {
                var res = await srv.GetByIdAsync(id, token);
                return Results.Ok(res);
            }
            catch (NotFoundException _)
            {
                return Results.NotFound();
            }
        }
    }
}
