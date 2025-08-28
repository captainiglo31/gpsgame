using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GpsGame.Application.Resources;
using GpsGame.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GpsGame.Tests.Api
{
    public class CollectIntegrationTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public CollectIntegrationTests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task Collect_Increments_Inventory_And_Returns_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Player-Token", "testing-token"); // passend zum Seed oben

            var nodeId = Guid.NewGuid();
            var req = new CollectRequestDto { PlayerId = _factory.SeededPlayerId, PlayerLatitude = 51.5, PlayerLongitude = 6.33, Amount = 5 };

            var resp = await client.PostAsJsonAsync($"/api/resources/{nodeId}/collect", req);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new Exception($"HTTP {(int)resp.StatusCode}: {resp.ReasonPhrase}\n{body}");
            }
            resp.EnsureSuccessStatusCode();

            // verify in DB
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var inv = await db.PlayerInventory.FirstOrDefaultAsync(
                x => x.PlayerId == _factory.SeededPlayerId && x.ResourceType == "iron");

            Assert.NotNull(inv);
            Assert.Equal(5, inv!.Amount);
        }
        
        
    }
}