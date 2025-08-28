using GpsGame.Api.Extensions;
using GpsGame.Infrastructure; 
using GpsGame.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Formatting.Json; // for structured JSON formatting
using GpsGame.Infrastructure.Seed; // for DB migration and seed extension
using FluentValidation;
using FluentValidation.AspNetCore;
using GpsGame.Application.Resources;
using GpsGame.Infrastructure.Services;
using GpsGame.Api.Auth;
using GpsGame.Application.Inventory;
using GpsGame.Application.Security;
using Microsoft.AspNetCore.Authentication;


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

// MVC Controllers
builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<GpsGame.Application.Players.PlayerCreateDto>();

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

// Fluent Validation
builder.Services.AddFluentValidationAutoValidation();

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

//using (var scope = app.Services.CreateScope())
//{
    //var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    //db.Database.Migrate(); // wendet fehlende Migrationen an
//}

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



// DB Seeder
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        await DbSeeder.SeedAsync(db, CancellationToken.None);
        logger.LogInformation("Database seeding completed.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database seeding failed.");
        throw;
    }
}


// Auth
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();


// Demo-Tokens beim Start ausgeben
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var demoTokens = db.Players
        .AsNoTracking()
        .Select(p => new { p.Username, p.ApiToken })
        .ToList();

    foreach (var dt in demoTokens)
    {
        logger.LogInformation("Demo player token: {Username} -> {Token}", dt.Username, dt.ApiToken);
    }
}

app.Run();
