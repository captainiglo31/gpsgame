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
    public class InventoryServiceConcurrencyTests
    {
        private static AppDbContext CreateDb(out SqliteConnection conn)
        {
            conn = new SqliteConnection("DataSource=:memory:");
            conn.Open();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(conn)
                .Options;
            var db = new AppDbContext(options);
            db.Database.EnsureCreated();
            return db;
        }

        [Fact]
        public async Task ConcurrentIncrements_AreSummed()
        {
            var db = CreateDb(out var conn);
            try
            {
                var playerId = Guid.NewGuid();
                db.Players.Add(new Player { Id = playerId, Username = "cc", Latitude = 0, Longitude = 0 });
                await db.SaveChangesAsync();

                var svc = new InventoryService(db);

                // 50 parallele Increments
                var tasks = Enumerable.Range(0, 50)
                    .Select(_ => svc.IncrementAsync(playerId, "iron", 1));
                await Task.WhenAll(tasks);

                var items = await svc.GetByPlayerAsync(playerId);
                Assert.Contains(items, x => x.ResourceType == "iron" && x.Amount == 50);
            }
            finally
            {
                conn.Dispose();
                db.Dispose();
            }
        }
    }
}