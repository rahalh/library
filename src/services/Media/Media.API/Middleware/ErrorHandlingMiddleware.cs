namespace Media.API.Middleware
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Serilog;

    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next) => this._next = next;

        public async Task Invoke(HttpContext ctx)
        {
            try
            {
                await this._next(ctx);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(ctx, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext ctx, Exception ex)
        {
            Log.Error(ex, "An unhandled error");
            return ctx.Response.WriteAsJsonAsync(new ProblemDetails()
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "An unexpected error occurred while processing your request."
            });
        }
    }
}
