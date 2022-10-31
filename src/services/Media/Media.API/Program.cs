using Confluent.Kafka;
using Media.API.Adapters;
using Media.API.Configuration;
using Media.API.Core;
using Media.API.Core.Interactors;
using Media.API.Middleware;
using Media.API.Ports.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Media.API.Ports.HTTP;
using Microsoft.Extensions.Configuration;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddSingleton(_ => new RedisSettings(builder.Configuration.GetConnectionString("Redis")));
builder.Services.AddSingleton(_ => new PostgresqlSettings(builder.Configuration.GetConnectionString("PostgreSQL")));

builder.Services.AddSingleton(_ => builder.Services.AddSingleton(_ =>
{
    var producerConfig = new ProducerConfig() {BootstrapServers = builder.Configuration.GetConnectionString("Kafka")};
    return new ProducerBuilder<Null, string>(producerConfig).Build();
}));

builder.Services.AddSingleton<ListMediaInteractor>();
builder.Services.AddSingleton<GetMediaInteractor>();
builder.Services.AddSingleton<CreateMediaInteractor>();
builder.Services.AddSingleton<DeleteMediaInteractor>();
builder.Services.AddSingleton<SetContentURLInteractor>();
builder.Services.AddSingleton<RemoveContentURLInteractor>();

builder.Services.AddSingleton<IMediaRepository, MediaPgRepository>();
builder.Services.AddSingleton<IEventProducer, KafkaEventProducer>();
builder.Services.Decorate<IMediaRepository, MediaRedisRepository>();
builder.Services.AddSingleton(Log.Logger);

builder.Services.AddHostedService<BackgroundKafkaConsumer>();

var app = builder.Build();
// TODO add swagger
// TODO document endpoints

app.UsePathBase("/api");

app.UseMiddleware<ErrorHandlingMiddleware>();
app.MapMediaEndpoints();

app.Run();

public partial class Program
{
}
