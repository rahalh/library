using FluentValidation;
using Media.API.Adapters;
using Media.API.Core;
using Media.API.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Media.API.Ports.HTTP;
using Media.API.Ports.HTTP.Endpoints;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(
        new JsonFormatter(),
        builder.Environment.IsProduction() ? LogEventLevel.Information : LogEventLevel.Debug)
    .CreateLogger();

builder.Services.AddSingleton<IMediaRepository, MediaRepository>();
builder.Services.AddSingleton<IMediaService, MediaService>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateRequest>();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.MapMediaEndpoints();

app.Run();
