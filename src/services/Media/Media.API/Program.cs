using Confluent.Kafka;
using FluentValidation;
using Media.API.Adapters;
using Media.API.Adapters.Kafka.Producer;
using Media.API.Core;
using Media.API.Middleware;
using Media.API.Ports.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Media.API.Ports.HTTP;
using Media.API.Ports.HTTP.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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

builder.Services.AddHostedService<BackgroundKafkaConsumer>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateRequest>();

var app = builder.Build();
// TODO add swagger
// TODO document endpoints

if (!app.Environment.IsProduction())
{
    app.UseDeveloperExceptionPage();
}

app.UseMiddleware<ErrorHandlingMiddleware>();
app.MapMediaEndpoints();

app.Run();

public partial class Program { }
