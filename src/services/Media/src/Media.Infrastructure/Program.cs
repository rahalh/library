using Confluent.Kafka;
using Media.Application.Interactors;
using Media.Application.Services;
using Media.Domain.Services;
using Media.Infrastructure.Adapters;
using Media.Infrastructure.Configuration;
using Media.Infrastructure.Middleware;
using Media.Infrastructure.Transport.Events;
using Media.Infrastructure.Transport.HTTP;
using Microsoft.Extensions.Options;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddOptions<RedisSettings>()
    .BindConfiguration("Redis")
    .ValidateDataAnnotations()
    .ValidateOnStart();


builder.Services.AddOptions<PostgresqlSettings>()
    .BindConfiguration("PostgreSQL")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<PostgresqlSettings>>().Value);
builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<RedisSettings>>().Value);

builder.Services.AddSingleton<IProducer<Null, string>>(_ =>
{
    var producerConfig = new ProducerConfig() {BootstrapServers = builder.Configuration.GetConnectionString("Kafka")};
    return new ProducerBuilder<Null, string>(producerConfig).Build();
});

//  By connecting here we are making sure that our service
//  cannot start until redis is ready. This might slow down startup,
//  but given that there is a delay on resolving the ip address
//  and then creating the connection it seems reasonable to move
//  that cost to startup instead of having the first request pay the
//  penalty.
builder.Services.AddSingleton<IConnectionMultiplexer>(resolver =>
{
    var settings = resolver.GetRequiredService<IOptions<RedisSettings>>().Value;
    return ConnectionMultiplexer.Connect(settings.ConnectionString);
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

app.UseMiddleware<ErrorHandlingMiddleware>();
app.MapMediaEndpoints();

app.Run();

public partial class Program {}
