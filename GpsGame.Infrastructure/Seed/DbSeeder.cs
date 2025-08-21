using System.Threading;
using System.Threading.Tasks;
using GpsGame.Infrastructure.Persistence;
using GpsGame.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GpsGame.Infrastructure.Seed
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken)
        {
            if (await context.Players.AnyAsync(cancellationToken))
                return;

            var players = new[]
            {
                new Player { Id = Guid.NewGuid(), Username = "demo1", Latitude = 51.5140, Longitude = 6.3290 },
                new Player { Id = Guid.NewGuid(), Username = "demo2", Latitude = 51.5150, Longitude = 6.3300 },
                new Player { Id = Guid.NewGuid(), Username = "demo3", Latitude = 51.5135, Longitude = 6.3285 },
                new Player { Id = Guid.NewGuid(), Username = "demo4", Latitude = 51.5160, Longitude = 6.3310 },
                new Player { Id = Guid.NewGuid(), Username = "demo5", Latitude = 51.5125, Longitude = 6.3275 }
            };

            await context.Players.AddRangeAsync(players, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}