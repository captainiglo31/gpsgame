using GpsGame.Application.FeatureFlags;
using GpsGame.Infrastructure.FeatureFlags;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GpsGame.Application.Resources;
using GpsGame.Infrastructure.Services;

namespace GpsGame.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("Db") 
                 ?? "Host=localhost;Port=5432;Database=gpsgame;Username=gps;Password=gpspwd";

        services.AddDbContext<Persistence.AppDbContext>(opt =>
            opt.UseNpgsql(cs));
        
        // Feature flags (read-only)
        services.AddScoped<IFeatureFlagReader, FeatureFlagReader>();
            
        // Resource query
        services.AddScoped<GpsGame.Application.Resources.IResourceQuery, GpsGame.Infrastructure.Resources.ResourceQuery>();
        
        // Resources Respawner
        services.AddScoped<IResourceRespawnService, ResourceRespawnService>();
        

        return services;
    }
}