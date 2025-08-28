using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GpsGame.Domain.Entities;
using GpsGame.Infrastructure.Persistence;
using GpsGame.Tests.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GpsGame.Tests.ApiTests
{
    public class GetInventoryEndpointTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public GetInventoryEndpointTests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task GetInventory_ReturnsAggregation()
        {
            // Seed zwei Ressourcentypen für den Player
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var iron = await db.PlayerInventory
                    .FirstOrDefaultAsync(x => x.PlayerId == _factory.SeededPlayerId && x.ResourceType == "iron");
                if (iron == null)
                {
                    db.PlayerInventory.Add(new PlayerInventory
                    {
                        Id = System.Guid.NewGuid(),
                        PlayerId = _factory.SeededPlayerId,
                        ResourceType = "iron",
                        Amount = 7,
                        UpdatedAt = System.DateTime.UtcNow
                    });
                }
                else
                {
                    iron.Amount = 7;
                    iron.UpdatedAt = System.DateTime.UtcNow;
                    db.Update(iron);
                }

                var copper = await db.PlayerInventory
                    .FirstOrDefaultAsync(x => x.PlayerId == _factory.SeededPlayerId && x.ResourceType == "copper");
                if (copper == null)
                {
                    db.PlayerInventory.Add(new PlayerInventory
                    {
                        Id = System.Guid.NewGuid(),
                        PlayerId = _factory.SeededPlayerId,
                        ResourceType = "copper",
                        Amount = 3,
                        UpdatedAt = System.DateTime.UtcNow
                    });
                }
                else
                {
                    copper.Amount = 3;
                    copper.UpdatedAt = System.DateTime.UtcNow;
                    db.Update(copper);
                }

                await db.SaveChangesAsync();
            }

            var client = _factory.CreateClient();
            var data = await client.GetFromJsonAsync<dynamic>($"/api/players/{_factory.SeededPlayerId}/inventory");

            // dynamic Auswertung: wir erwarten zwei Einträge iron=7, copper=3
            var items = ((System.Text.Json.JsonElement)data).EnumerateArray()
                .Select(el => new
                {
                    ResourceType = el.GetProperty("resourceType").GetString() ?? el.GetProperty("ResourceType").GetString(),
                    Amount = el.GetProperty("amount").GetInt64()
                })
                .ToList();

            Assert.Contains(items, x => (x.ResourceType == "iron" || x.ResourceType == "Iron") && x.Amount == 7);
            Assert.Contains(items, x => (x.ResourceType == "copper" || x.ResourceType == "Copper") && x.Amount == 3);
        }
    }
}
