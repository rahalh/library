using Confluent.Kafka;
using Media.Application.Interactors;
using Media.Application.Services;
using Media.Domain.Services;
using Media.Infrastructure.Adapters;
using Media.Infrastructure.Configuration;
using Media.Infrastructure.Middleware;
using Media.Infrastructure.Transport.Events;
using Media.Infrastructure.Transport.HTTP;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddOptions<RedisSettings>()
    .BindConfiguration("ConnectionStrings:Redis")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<PostgresqlSettings>()
    .BindConfiguration("ConnectionStrings:PostgreSQL")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<PostgresqlSettings>>().Value);
builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<RedisSettings>>().Value);

builder.Services.AddSingleton<IProducer<Null, string>>(_ =>
{
    var producerConfig = new ProducerConfig()
    {
        BootstrapServers = builder.Configuration.GetConnectionString("Kafka")
    };
    return new ProducerBuilder<Null, string>(producerConfig).Build();
});

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

app.UseMiddleware<ErrorHandlingMiddleware>();
app.MapMediaEndpoints();

app.Run();

public partial class Program { }
