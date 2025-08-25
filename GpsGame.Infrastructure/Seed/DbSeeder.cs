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
            var now = DateTime.UtcNow;

            // 1) Players: fehlende Demo-User ergänzen (idempotent per Username)
            var demoUsers = new[]
            {
                new { Username = "demo1", Lat = 51.5140, Lng = 6.3290 },
                new { Username = "demo2", Lat = 51.5150, Lng = 6.3300 },
                new { Username = "demo3", Lat = 51.5135, Lng = 6.3285 },
                new { Username = "demo4", Lat = 51.5160, Lng = 6.3310 },
                new { Username = "demo5", Lat = 51.5125, Lng = 6.3275 }
            };

            var existingNames = await context.Players
                .AsNoTracking()
                .Select(p => p.Username)
                .ToListAsync(cancellationToken);

            var missingPlayers = demoUsers
                .Where(d => !existingNames.Contains(d.Username))
                .Select(d => new Player
                {
                    Id = Guid.NewGuid(),
                    Username = d.Username,
                    Latitude = d.Lat,
                    Longitude = d.Lng,
                    CreatedUtc = now
                })
                .ToList();

            if (missingPlayers.Count > 0)
            {
                await context.Players.AddRangeAsync(missingPlayers, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }
            
            // Tokens für Demo-Player sicherstellen
            var playersNeedingToken = await context.Players
                .Where(p => p.ApiToken == null || p.ApiToken == "")
                .ToListAsync(cancellationToken);

            foreach (var p in playersNeedingToken)
            {
                p.ApiToken = Guid.NewGuid().ToString("N"); // simpel & ausreichend für MVP
                p.UpdatedUtc = now;
            }

            if (playersNeedingToken.Count > 0)
                await context.SaveChangesAsync(cancellationToken);

            // 2) FeatureFlags: setzen, falls Key fehlt (idempotent per Key)
            async Task EnsureFlagAsync(string key, bool enabled)
            {
                var exists = await context.FeatureFlags
                    .AsNoTracking()
                    .AnyAsync(f => f.Key == key, cancellationToken);
                if (!exists)
                {
                    context.FeatureFlags.Add(new FeatureFlag
                    {
                        Id = Guid.NewGuid(),
                        Key = key,
                        Enabled = enabled,
                        CreatedUtc = now,
                        UpdatedUtc = now
                    });
                }
            }

            await EnsureFlagAsync("resources_enabled", true);
            await EnsureFlagAsync("pvp_enabled", false);

            // Nur speichern, wenn Flags neu hinzugekommen sind
            if (context.ChangeTracker.HasChanges())
                await context.SaveChangesAsync(cancellationToken);

            // 3) ResourceNodes: Beispiel-Nodes ergänzen, falls Tabelle leer (oder du prüfst gezielt per Typ/Koordinate)
            var hasAnyNode = await context.ResourceNodes.AsNoTracking().AnyAsync(cancellationToken);
            if (!hasAnyNode)
            {
                var nodes = new[]
                {
                    new ResourceNode
                    {
                        Id = Guid.NewGuid(),
                        Type = "Wood",
                        Latitude = 51.5149,
                        Longitude = 6.3301,
                        Amount = 20,
                        MaxAmount = 20,
                        CreatedUtc = now,
                        UpdatedUtc = now
                    },
                    new ResourceNode
                    {
                        Id = Guid.NewGuid(),
                        Type = "Iron",
                        Latitude = 51.5150,
                        Longitude = 6.3305,
                        Amount = 15,
                        MaxAmount = 15,
                        CreatedUtc = now,
                        UpdatedUtc = now
                    },
                    new ResourceNode
                    {
                        Id = Guid.NewGuid(),
                        Type = "Stone",
                        Latitude = 51.5152,
                        Longitude = 6.3306,
                        Amount = 15,
                        MaxAmount = 15,
                        CreatedUtc = now,
                        UpdatedUtc = now
                    }
                };

                await context.ResourceNodes.AddRangeAsync(nodes, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
