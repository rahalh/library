namespace Media.API.Middleware
{
    using System;
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Core.Exceptions;
    using Infrastructure.Exceptions;
    using Microsoft.AspNetCore.Http;
    using Serilog;

    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger logger) {
            this._next = next;
            this.logger = logger.ForContext<ErrorHandlingMiddleware>();
        }

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

        private async Task HandleExceptionAsync(HttpContext ctx, Exception ex)
        {
            var code = HttpStatusCode.InternalServerError;
            var message = "An unhandled error has occurred.";

            if (ex is ValidationException)
            {
                code = HttpStatusCode.BadRequest;
                message = ex.Message;
                this.logger.Error(ex, ex.Message);
            } else if (ex is ConflictException)
            {
                code = HttpStatusCode.Conflict;
                message = ex.Message;
                this.logger.Error(ex, ex.Message);
            } else if (ex is NotFoundException)
            {
                code = HttpStatusCode.NotFound;
                message = ex.Message;
            } else
            {
                this.logger.Error(ex, "Middleware caught an unhandled exception.");
            }

            var result = JsonSerializer.Serialize(new {code, message});
            ctx.Response.ContentType = "application/json";
            ctx.Response.StatusCode = (int)code;
            await ctx.Response.WriteAsync(result);
        }
    }
}
