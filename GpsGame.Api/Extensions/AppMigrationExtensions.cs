using System.Threading;
using System.Threading.Tasks;
using GpsGame.Infrastructure.Persistence;
using GpsGame.Infrastructure.Seed;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GpsGame.Api.Extensions
{
    public static class AppMigrationExtensions
    {
        public static async Task MigrateAndSeedAsync(this WebApplication app, CancellationToken ct = default)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync(ct);
            await DbSeeder.SeedAsync(db, ct);
        }
    }
}