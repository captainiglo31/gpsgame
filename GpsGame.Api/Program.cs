using GpsGame.Api.Extensions;
using GpsGame.Infrastructure; 
using GpsGame.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Formatting.Json; // for structured JSON formatting
using GpsGame.Infrastructure.Seed; // for DB migration and seed extension


var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithThreadId()
    .WriteTo.Console()   
    .CreateLogger();

builder.Host.UseSerilog();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health
builder.Services.AddHealthChecks();

// DI Wirewing
builder.Services.AddInfrastructure(builder.Configuration);

// CORS: unified policy for Unity dev origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUnityDev", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",
            "http://localhost:5173",
            "http://localhost:4200",
            "http://127.0.0.1:3000",
            "http://127.0.0.1:5173",
            "http://127.0.0.1:4200"
        )
        .WithMethods("GET", "POST", "PUT", "DELETE")
        .AllowAnyHeader();
    });
});

// CORS: unify policy for Unity dev origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUnityDev", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",
            "http://localhost:5173",
            "http://localhost:4200",
            "http://127.0.0.1:3000",
            "http://127.0.0.1:5173",
            "http://127.0.0.1:4200"
        )
        .WithMethods("GET", "POST", "PUT", "DELETE")
        .AllowAnyHeader();
    });
});

var app = builder.Build();

// Apply migrations and seed database if empty
app.MigrateAndSeedAsync().GetAwaiter().GetResult();

// Swagger nur in Dev (für MVP auch in Prod möglich, aber hier dev-first)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

// Enable CORS globally before endpoints
app.UseCors("AllowUnityDev");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/version", () => new { name = "GpsGame.Api", version = "0.1.0", framework = "net8.0" });
app.MapHealthChecks("/healthz");

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

// Nach dem Build:
app.MapGet("/players", async (AppDbContext db) =>
    await db.Players
        .OrderByDescending(p => p.CreatedUtc)
        .Take(50)
        .ToListAsync());


app.MapPost("/players", async (AppDbContext db, string username, double lat, double lng) =>
{
    var p = new GpsGame.Domain.Entities.Player
    {
        Id = Guid.NewGuid(),
        Username = username,
        Latitude = lat,
        Longitude = lng
    };
    db.Players.Add(p);
    await db.SaveChangesAsync();
    return Results.Created($"/players/{p.Id}", p);
});

using (var scope = app.Services.CreateScope())
{
    var reader = scope.ServiceProvider.GetRequiredService<GpsGame.Application.FeatureFlags.IFeatureFlagReader>();
    var all = await reader.GetAllAsync();
    Log.Information("FeatureFlags loaded at startup: {@Flags}", all);
}

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
