namespace Blob.API.Middlewares
{
    using System;
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Core.Exceptions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Serilog;
    using Serilog.Sinks.Http.Private.NonDurable;

    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger logger;

        public ErrorHandlingMiddleware(ILogger logger, RequestDelegate next)
        {
            this.logger = logger;
            this._next = next;
        }

        public async Task Invoke(HttpContext ctx)
        {
            try
            {
                await this._next(ctx);
            }
            catch (Exception ex)
            {
                await this.HandleExceptionAsync(ctx, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext ctx, Exception ex)
        {
            var code = HttpStatusCode.InternalServerError;
            var message = "An unhandled error has occurred.";

            if (ex is ValidationException || ex is BadHttpRequestException)
            {
                code = HttpStatusCode.BadRequest;
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
