using Blob.API.Adapters;
using Blob.API.Config;
using Blob.API.Core;
using Blob.API.Middlewares;
using Blob.API.Ports.HTTP;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddSingleton<DDBSettings>(_ => new DDBSettings(builder.Configuration["AWS:DDB:TableName"], builder.Configuration["AWS:DDB:ServiceURL"]));
builder.Services.AddSingleton<S3Settings>(_ => new S3Settings(builder.Configuration["AWS:S3:BucketName"]) {Prefix = builder.Configuration["AWS:S3:Prefix"]});

builder.Services.AddSingleton(Log.Logger);
builder.Services.AddSingleton<IFileStore, S3FileStore>();
builder.Services.AddSingleton<IBlobRepository, DDBRepository>();
builder.Services.AddSingleton<IBlobService, BlobService>();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapBlobEndpoints();
app.Run();

public partial class Program { }
