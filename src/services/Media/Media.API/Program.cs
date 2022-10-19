using FluentValidation;
using Media.API.Adapters;
using Media.API.Core;
using Media.API.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Media.API.Ports.HTTP;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IMediaRepository, MediaRepository>();
builder.Services.AddSingleton<IMediaService, MediaService>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateRequest>();

var app = builder.Build();

app.MapMediaEndpoints();

app.Run();
