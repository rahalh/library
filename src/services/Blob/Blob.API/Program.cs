using Blob.API.Adapters;
using Blob.API.Configuration;
using Blob.API.Core;
using Blob.API.Core.Interactors;
using Blob.API.Middlewares;
using Blob.API.Ports.HTTP;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddSingleton<DDBSettings>(_ => new DDBSettings(builder.Configuration["AWS:DDB:TableName"], builder.Configuration["AWS:DDB:ServiceURL"]));
builder.Services.AddSingleton<S3Settings>(_ => new S3Settings(builder.Configuration["AWS:S3:BucketName"], builder.Configuration["AWS:S3:StorageDomain"]) {Prefix = builder.Configuration["AWS:S3:Prefix"]});

builder.Services.AddSingleton<DeleteBlobInteractor>();
builder.Services.AddSingleton<CreateBlobInteractor>();
builder.Services.AddSingleton<GetBlobByIdInteractor>();

builder.Services.AddSingleton(Log.Logger);
builder.Services.AddSingleton<IFileStore, S3FileStore>();
builder.Services.AddSingleton<IBlobRepository, DDBRepository>();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapBlobEndpoints();
app.Run();
