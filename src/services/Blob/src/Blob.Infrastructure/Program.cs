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
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddSingleton(_ => new DDBSettings(builder.Configuration["AWS:DDB:TableName"], builder.Configuration["AWS:DDB:ServiceURL"]));
builder.Services.AddSingleton(_ => new S3Settings(builder.Configuration["AWS:S3:BucketName"], builder.Configuration["AWS:S3:StorageDomain"]) {Prefix = builder.Configuration["AWS:S3:Prefix"]});

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
    var logger = sp.GetRequiredService<Serilog.ILogger>();
    var eventProducer = sp.GetRequiredService<IEventProducer>();
    var fileStore = sp.GetRequiredService<IFileStore>();
    var repo = sp.GetRequiredService<IBlobRepository>();
    return new StoreBlobInteractor(logger, repo, fileStore, eventProducer, builder.Configuration["AWS:S3:StorageDomain"]);
});
builder.Services.AddSingleton<GetBlobByIdInteractor>();
builder.Services.AddSingleton<DeleteBlobSagaInteractor>();

builder.Services.AddHostedService<BackgroundKafkaConsumer>();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapBlobEndpoints();
app.Run();

public partial class Program { }
