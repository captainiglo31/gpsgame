using System;
using GpsGame.Application.Resources;
using GpsGame.Domain.Entities;
using GpsGame.Infrastructure.Persistence;
using GpsGame.Tests.TestDoubles;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using GpsGame.Application.FeatureFlags;
using GpsGame.Application.Resources;
using GpsGame.Tests.TestDoubles;
using Microsoft.AspNetCore.Mvc;

namespace GpsGame.Tests.Api
{
    public class ApiFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection? _conn;

        public Guid SeededPlayerId { get; private set; } = Guid.NewGuid();
        public FakeCollector FakeCollector { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<AppDbContext>();
                services.RemoveAll<IDbContextFactory<AppDbContext>>();
                services.RemoveAll<IResourceCollector>();
                services.RemoveAll<IResourceQuery>();
                services.RemoveAll<IFeatureFlagReader>();

                // Validation im Test nicht auto-400n lassen
                services.Configure<ApiBehaviorOptions>(o => o.SuppressModelStateInvalidFilter = true);

                // Fakes
                services.AddSingleton<IResourceCollector>(FakeCollector);
                services.AddSingleton<IResourceQuery, FakeResourceQuery>();
                services.AddSingleton<IFeatureFlagReader, AlwaysOnFeatureFlags>();

                _conn = new SqliteConnection("DataSource=:memory:");
                _conn.Open();
                services.AddDbContext<AppDbContext>(o => o.UseSqlite(_conn));

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();

                // Seed Player + Token (falls im Modell vorhanden)
                SeededPlayerId = Guid.NewGuid();
                var seededToken = "testing-token";
                db.Players.Add(new Player
                {
                    Id = SeededPlayerId,
                    Username = "api_integration",
                    Latitude = 51.5,
                    Longitude = 6.33,
                    ApiToken = seededToken // falls Property vorhanden
                });
                db.SaveChanges();

                FakeCollector.PlayerId = SeededPlayerId;

                SeededPlayerId = Guid.NewGuid();
                db.Players.Add(new Player
                {
                    Id = SeededPlayerId,
                    Username = "api_integration",
                    Latitude = 51.5,
                    Longitude = 6.33
                });
                db.SaveChanges();

                // FakeCollector an den Seeded Player binden
                FakeCollector.PlayerId = SeededPlayerId;
                // optional: FakeCollector.ResourceType = "iron"; FakeCollector.Collected = 5;
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _conn?.Dispose();
        }
    }
}
