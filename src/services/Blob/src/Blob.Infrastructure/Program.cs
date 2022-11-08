using Blob.Application.Interactors;
using Blob.Application.Services;
using Blob.Domain.Services;
using Blob.Infrastructure.Adapters;
using Blob.Infrastructure.Configuration;
using Blob.Infrastructure.Middlewares;
using Blob.Infrastructure.Transport.Events;
using Blob.Infrastructure.Transport.HTTP;
using Confluent.Kafka;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddOptions<S3Settings>()
    .BindConfiguration("AWS:S3")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<DDBSettings>()
    .BindConfiguration("AWS:DDB")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<S3Settings>>().Value);
builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<DDBSettings>>().Value);

builder.Services.AddSingleton<IProducer<Null, string>>(_ =>
{
    var producerConfig = new ProducerConfig()
    {
        BootstrapServers = builder.Configuration.GetConnectionString("Kafka")
    };
    return new ProducerBuilder<Null, string>(producerConfig).Build();
});

builder.Services.AddSingleton(Log.Logger);

builder.Services.AddSingleton<IEventProducer, KafkaEventProducer>();
builder.Services.AddSingleton<IFileStore, S3FileStore>();
builder.Services.AddSingleton<IBlobRepository, DDBRepository>();

builder.Services.AddSingleton<DeleteBlobInteractor>();
builder.Services.AddSingleton<StoreBlobInteractor>(sp =>
{
    var settings = sp.GetRequiredService<S3Settings>();
    return new StoreBlobInteractor(
        logger: sp.GetRequiredService<Serilog.ILogger>(),
        repo: sp.GetRequiredService<IBlobRepository>(),
        fileStore: sp.GetRequiredService<IFileStore>(),
        eventProducer: sp.GetRequiredService<IEventProducer>(),
        settings.StorageDomain,
        settings.Prefix ?? string.Empty);
});

builder.Services.AddSingleton<GetBlobByIdInteractor>();
builder.Services.AddSingleton<DeleteBlobSagaInteractor>();

builder.Services.AddHostedService<BackgroundKafkaConsumer>();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapBlobEndpoints();
app.Run();

public partial class Program { }
