ARG BUILD_CONFIGURATION=Release
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
WORKDIR /app
EXPOSE 443 80

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine as build
WORKDIR /app
COPY *.sln .
COPY src/Blob.Application/*.csproj src/Blob.Application/
COPY src/Blob.Domain/*.csproj src/Blob.Domain/
COPY src/Blob.Infrastructure/*.csproj src/Blob.Infrastructure/
COPY tests/Blob.Infrastructure.Tests/*.csproj tests/Blob.Infrastructure.Tests/
COPY tests/Blob.Application.Tests/*.csproj tests/Blob.Application.Tests/
RUN dotnet restore

COPY . .
WORKDIR /app/src/Blob.Infrastructure
RUN dotnet build "Blob.Infrastructure.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

FROM build AS publish
RUN dotnet publish "Blob.Infrastructure.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet", "/app/Blob.Infrastructure.dll" ]
