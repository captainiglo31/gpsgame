# build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . ./GpsGame.Api
COPY ../GpsGame.Domain ./GpsGame.Domain
COPY ../GpsGame.Application ./GpsGame.Application
COPY ../GpsGame.Infrastructure ./GpsGame.Infrastructure
RUN dotnet restore ./GpsGame.Api/GpsGame.Api.csproj
RUN dotnet publish ./GpsGame.Api/GpsGame.Api.csproj -c Release -o /app

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "GpsGame.Api.dll"]