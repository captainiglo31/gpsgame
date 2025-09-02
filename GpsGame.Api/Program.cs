using System.Text.Json;
using GpsGame.Api.Extensions;
using GpsGame.Infrastructure;
using GpsGame.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;
using FluentValidation;
using FluentValidation.AspNetCore;
using GpsGame.Application.Resources;
using GpsGame.Infrastructure.Services;
using GpsGame.Api.Auth;
using GpsGame.Application.Inventory;
using GpsGame.Application.Security;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Serilog
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

// MVC + Validation
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);
builder.Services.AddValidatorsFromAssemblyContaining<GpsGame.Application.Players.PlayerCreateDto>();
builder.Services.AddFluentValidationAutoValidation();

// Services
builder.Services.AddScoped<IResourceCollector, ResourceCollector>();
builder.Services.AddScoped<IResourceRules, ResourceRules>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

// Auth
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentPlayerAccessor, CurrentPlayerAccessor>();
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = PlayerTokenAuthenticationHandler.Scheme;
        options.DefaultChallengeScheme = PlayerTokenAuthenticationHandler.Scheme;
    })
    .AddScheme<AuthenticationSchemeOptions, PlayerTokenAuthenticationHandler>(
        PlayerTokenAuthenticationHandler.Scheme, _ => { });
builder.Services.AddAuthorization();

// Health
builder.Services.AddHealthChecks();

// CORS (einmal, nicht doppelt)
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

// >>> WICHTIG: Infrastruktur (inkl. Npgsql/DbContext) NUR außerhalb von Tests registrieren
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddInfrastructure(builder.Configuration);
}

var app = builder.Build();

// >>> WICHTIG: Migration/Seeding NUR außerhalb von Tests
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate(); // fehlende Migrationen anwenden
    }

    // Optional: zentrale Migrate+Seed Extension
    await app.MigrateAndSeedAsync();

    // Demo-Tokens loggen
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var demoTokens = db.Players.AsNoTracking().Select(p => new { p.Username, p.ApiToken }).ToList();
        foreach (var dt in demoTokens)
            logger.LogInformation("Demo player token: {Username} -> {Token}", dt.Username, dt.ApiToken);
    }
}

// Swagger in Dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowUnityDev");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Für WebApplicationFactory
public partial class Program { }
