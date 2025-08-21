using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GpsGame.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("Db") 
                 ?? "Host=localhost;Port=5432;Database=gpsgame;Username=gps;Password=gpspwd";

        services.AddDbContext<Persistence.AppDbContext>(opt =>
            opt.UseNpgsql(cs));

        return services;
    }
}