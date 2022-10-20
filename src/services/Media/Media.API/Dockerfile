FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine as build
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -o /app/media-service

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine as runtime
WORKDIR /app
COPY --from=build /app/media-service /app

# Expose port 8080 to the outside world
EXPOSE 8080

ENTRYPOINT [ "dotnet", "/app/Media.API.dll" ]
