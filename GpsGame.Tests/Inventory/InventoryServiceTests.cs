using System;
using System.Linq;
using System.Threading.Tasks;
using GpsGame.Domain.Entities;
using GpsGame.Infrastructure.Persistence;
using GpsGame.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GpsGame.Tests.Inventory
{
    public class InventoryServiceTests
    {
        private static AppDbContext CreateDb(out SqliteConnection conn)
        {
            conn = new SqliteConnection("DataSource=:memory:");
            conn.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(conn)
                .Options;

            var db = new AppDbContext(options);
            db.Database.EnsureCreated(); // build model into sqlite

            return db;
        }

        [Fact]
        public async Task Increment_Upserts_And_Aggregates()
        {
            var db = CreateDb(out var conn);
            try
            {
                // seed a player
                var playerId = Guid.NewGuid();
                db.Players.Add(new Player
                {
                    Id = playerId,
                    Username = "unit_tester",
                    Latitude = 51.5,
                    Longitude = 6.33
                });
                await db.SaveChangesAsync();

                var svc = new InventoryService(db);

                await svc.IncrementAsync(playerId, "iron", 5);
                await svc.IncrementAsync(playerId, "iron", 2);
                await svc.AddAsync(playerId, "copper", 1);

                var items = await svc.GetByPlayerAsync(playerId);
                Assert.Contains(items, x => x.ResourceType == "iron" && x.Amount == 7);
                Assert.Contains(items, x => x.ResourceType == "copper" && x.Amount == 1);
            }
            finally
            {
                conn.Dispose();
                db.Dispose();
            }
        }
        
        [Fact]
        public async Task GetAggregatedByPlayerAsync_GroupsAndSumsByResourceType()
        {
            using var db = CreateDb(out var conn);
            await using var _ = conn;

            var playerId = Guid.NewGuid();

            db.Players.Add(new Player { Id = playerId, Username = "agg_tester", Latitude = 0, Longitude = 0 });
            await db.SaveChangesAsync();

            var svc = new InventoryService(db);

            await svc.IncrementAsync(playerId, "Iron", 5);
            await svc.IncrementAsync(playerId, "Iron", 7);   // ergibt 12 in derselben Zeile
            await svc.IncrementAsync(playerId, "Stone", 3);

            var result = await svc.GetAggregatedByPlayerAsync(playerId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            var iron = result.Single(x => x.ResourceType == "Iron");
            var stone = result.Single(x => x.ResourceType == "Stone");

            Assert.Equal(12, iron.Amount);
            Assert.Equal(3, stone.Amount);
        }
    }
}