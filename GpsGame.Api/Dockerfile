# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Nur Projektdateien kopieren (besseres Caching)
COPY ./GpsGame.sln ./
COPY ./GpsGame.Api/GpsGame.Api.csproj ./GpsGame.Api/
COPY ./GpsGame.Domain/GpsGame.Domain.csproj ./GpsGame.Domain/
COPY ./GpsGame.Application/GpsGame.Application.csproj ./GpsGame.Application/
COPY ./GpsGame.Infrastructure/GpsGame.Infrastructure.csproj ./GpsGame.Infrastructure/

RUN dotnet restore ./GpsGame.Api/GpsGame.Api.csproj

# Jetzt den restlichen Source kopieren
COPY . .

RUN dotnet publish ./GpsGame.Api/GpsGame.Api.csproj -c Release -o /app

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "GpsGame.Api.dll"]
