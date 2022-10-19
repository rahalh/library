using FluentValidation;
using Media.API.Adapters;
using Media.API.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Media.API.Ports.HTTP;
using Media.API.Ports.HTTP.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IMediaRepository, MediaRepository>();
builder.Services.AddSingleton<IMediaService, MediaService>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateRequest>();

var app = builder.Build();

app.MapMediaEndpoints();

app.Run();
