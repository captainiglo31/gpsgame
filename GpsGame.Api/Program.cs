using GpsGame.Infrastructure; 
using GpsGame.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health
builder.Services.AddHealthChecks();

// DI Wirewing
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Swagger nur in Dev (für MVP auch in Prod möglich, aber hier dev-first)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

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

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
