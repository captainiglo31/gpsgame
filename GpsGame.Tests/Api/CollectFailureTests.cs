using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GpsGame.Application.Resources;
using GpsGame.Infrastructure.Persistence;
using GpsGame.Tests.Api;
using GpsGame.Tests.TestDoubles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace GpsGame.Tests.ApiTests
{
    public class CollectFailureTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public CollectFailureTests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task Collect_Failure_NoInventoryChange()
        {
            // Factory mit AlwaysFailCollector überschreiben
            var factory = _factory.WithWebHostBuilder(b => b.ConfigureServices(s =>
            {
                s.RemoveAll<IResourceCollector>();
                s.AddSingleton<IResourceCollector, AlwaysFailCollector>();
            }));

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Player-Token", "testing-token");

            var req = new CollectRequestDto
            {
                PlayerId = _factory.SeededPlayerId,
                PlayerLatitude = 51.5,
                PlayerLongitude = 6.33,
                Amount = 5
            };

            var resp = await client.PostAsJsonAsync($"/api/resources/{Guid.NewGuid()}/collect", req);
            Assert.False(resp.IsSuccessStatusCode); // 400/429 etc.

            // Inventory bleibt unverändert
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var inv = await db.PlayerInventory.FirstOrDefaultAsync(x => x.PlayerId == _factory.SeededPlayerId);
            Assert.Null(inv);
        }
    }
}