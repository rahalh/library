using FluentValidation;
using Media.API.Adapters;
using Media.API.Core;
using Media.API.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Media.API.Ports.HTTP;
using Media.API.Ports.HTTP.Handlers;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddSingleton<IMediaRepository, MediaPgRepository>();
builder.Services.AddSingleton<IMediaEventBus, MediaKafkaEventBus>();
builder.Services.Decorate<IMediaRepository, MediaRedisRepository>();
builder.Services.AddSingleton<IMediaService, MediaService>();
builder.Services.AddSingleton(Log.Logger);

builder.Services.AddValidatorsFromAssemblyContaining<CreateRequest>();

var app = builder.Build();
// TODO add swagger
// TODO document endpoints

app.UseMiddleware<ErrorHandlingMiddleware>();
app.MapMediaEndpoints();

app.Run();
