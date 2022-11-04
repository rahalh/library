ARG BUILD_CONFIGURATION=Release
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
WORKDIR /app
EXPOSE 443 80

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine as build
WORKDIR /app
COPY *.sln .
COPY src/Media.Application/*.csproj src/Media.Application/
COPY src/Media.Domain/*.csproj src/Media.Domain/
COPY src/Media.Infrastructure/*.csproj src/Media.Infrastructure/
COPY tests/Media.Infrastructure.Tests/*.csproj tests/Media.Infrastructure.Tests/
COPY tests/Media.Application.Tests/*.csproj tests/Media.Application.Tests/
RUN dotnet restore

COPY . .
WORKDIR /app/src/Media.Infrastructure
RUN dotnet build "Media.Infrastructure.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

FROM build AS publish
RUN dotnet publish "Media.Infrastructure.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet", "/app/Media.Infrastructure.dll" ]
