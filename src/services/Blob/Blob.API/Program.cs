using Blob.API.Adapters;
using Blob.API.Configuration;
using Blob.API.Core;
using Blob.API.Core.Interactors;
using Blob.API.Middlewares;
using Blob.API.Ports.Events;
using Blob.API.Ports.HTTP;
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
builder.Services.AddSingleton<StoreBlobInteractor>();
builder.Services.AddSingleton<GetBlobByIdInteractor>();
builder.Services.AddSingleton<SAGADeleteBlobInteractor>();

builder.Services.AddHostedService<BackgroundKafkaConsumer>();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapBlobEndpoints();
app.Run();
