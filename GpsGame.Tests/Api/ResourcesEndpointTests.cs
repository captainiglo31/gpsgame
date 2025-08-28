using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using GpsGame.Application.FeatureFlags;
using GpsGame.Application.Resources;
using GpsGame.Tests.Api;
using GpsGame.Tests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace GpsGame.Tests.ApiTests
{
    public class ResourcesEndpointTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        public ResourcesEndpointTests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task GetResources_Returns200_WhenFeatureEnabled_ValidBBox()
        {
            // Standard-ApiFactory nutzt AlwaysOnFeatureFlags; wir überschreiben nur die Query, um sicher zu sein
            var recQuery = new RecordingResourceQuery();
            var factory = _factory.WithWebHostBuilder(b => b.ConfigureServices(s =>
            {
                s.RemoveAll<IResourceQuery>();
                s.AddSingleton<IResourceQuery>(recQuery);
            }));

            var client = factory.CreateClient();

            var url = "/api/resources?minLat=51.51&minLng=6.32&maxLat=51.52&maxLng=6.33";
            var resp = await client.GetAsync(url);

            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            // Body: JSON-Array (leer, weil RecordingResourceQuery leer liefert)
            var text = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(text);
            Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);

            // Verifizieren, dass BBox weitergereicht wurde
            Assert.Equal(51.51, recQuery.LastMinLat);
            Assert.Equal(6.32, recQuery.LastMinLng);
            Assert.Equal(51.52, recQuery.LastMaxLat);
            Assert.Equal(6.33, recQuery.LastMaxLng);
        }

        [Fact]
        public async Task GetResources_Returns400_WhenLatitudeOutOfRange()
        {
            var client = _factory.CreateClient();
            // minLat absichtlich < -90
            var url = "/api/resources?minLat=-200&minLng=6.32&maxLat=51.52&maxLng=6.33";
            var resp = await client.GetAsync(url);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task GetResources_Returns400_WhenMinGreaterThanMax()
        {
            var client = _factory.CreateClient();
            // minLat >= maxLat
            var url = "/api/resources?minLat=51.53&minLng=6.32&maxLat=51.52&maxLng=6.33";
            var resp = await client.GetAsync(url);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task GetResources_Returns404_WhenFeatureDisabled()
        {
            var factory = _factory.WithWebHostBuilder(b => b.ConfigureServices(s =>
            {
                s.RemoveAll<IFeatureFlagReader>();
                s.AddSingleton<IFeatureFlagReader, FeatureFlagsOff>();
            }));

            var client = factory.CreateClient();
            var url = "/api/resources?minLat=51.51&minLng=6.32&maxLat=51.52&maxLng=6.33";
            var resp = await client.GetAsync(url);

            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task GetResources_ReturnsJsonArray_EvenWhenEmpty()
        {
            var recQuery = new RecordingResourceQuery();
            var factory = _factory.WithWebHostBuilder(b => b.ConfigureServices(s =>
            {
                s.RemoveAll<IResourceQuery>();
                s.AddSingleton<IResourceQuery>(recQuery);
            }));

            var client = factory.CreateClient();
            var url = "/api/resources?minLat=51.50&minLng=6.30&maxLat=51.55&maxLng=6.35";
            var data = await client.GetFromJsonAsync<JsonElement>(url);

            Assert.Equal(JsonValueKind.Array, data.ValueKind);
            Assert.Equal(0, data.GetArrayLength()); // leer, weil Fake leer liefert
        }
    }
}
