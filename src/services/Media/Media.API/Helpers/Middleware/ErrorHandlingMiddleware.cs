namespace Media.API.Middleware
{
    using System;
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Adapters.Exceptions;
    using Core.Helpers.Exceptions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
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

            if (ex is BadRequestException)
            {
                code = HttpStatusCode.BadRequest;
                message = ex.Message;
                this.logger.Error(ex, "Invalid operation or bad request details.");
            } else if (ex is EntityExistsException)
            {
                code = HttpStatusCode.Conflict;
                message = "Resource already exists";
                this.logger.Error(ex, "Entity already exists");
            } else if (ex is NotFoundException)
            {
                code = HttpStatusCode.NotFound;
                message = "Resource not found";
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
