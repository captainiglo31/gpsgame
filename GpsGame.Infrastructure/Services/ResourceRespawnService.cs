using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GpsGame.Application.Resources;
using GpsGame.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GpsGame.Infrastructure.Services
{
    /// <summary>
    /// Lazy respawn: refill nodes when they are queried or touched,
    /// without requiring a background job.
    /// </summary>
    public sealed class ResourceRespawnService : IResourceRespawnService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<ResourceRespawnService> _log;

        public ResourceRespawnService(AppDbContext db, ILogger<ResourceRespawnService> log)
        {
            _db = db;
            _log = log;
        }

        public async Task<int> RespawnDueAsync(
            double minLat, double minLng, double maxLat, double maxLng,
            CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            // EF Core 7/8: ExecuteUpdateAsync -> single SQL UPDATE (atomic)
            var affected = await _db.ResourceNodes
                .Where(n =>
                    n.Amount == 0 &&
                    n.RespawnAtUtc != null &&
                    n.RespawnAtUtc <= now &&
                    n.Latitude  >= minLat && n.Latitude  <= maxLat &&
                    n.Longitude >= minLng && n.Longitude <= maxLng)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(n => n.Amount,    n => n.MaxAmount)
                    .SetProperty(n => n.RespawnAtUtc, n => null),
                    cancellationToken: ct);

            if (affected > 0)
                _log.LogInformation("Respawned {Count} nodes in bbox [{MinLat},{MinLng}]–[{MaxLat},{MaxLng}].",
                    affected, minLat, minLng, maxLat, maxLng);

            return affected;
        }

        public async Task<bool> RespawnIfDueAsync(Guid nodeId, CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            var affected = await _db.ResourceNodes
                .Where(n => n.Id == nodeId &&
                            n.Amount == 0 &&
                            n.RespawnAtUtc != null &&
                            n.RespawnAtUtc <= now)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(n => n.Amount,    n => n.MaxAmount)
                    .SetProperty(n => n.RespawnAtUtc, n => null),
                    cancellationToken: ct);

            if (affected > 0)
                _log.LogDebug("Respawned node {NodeId}.", nodeId);

            return affected > 0;
        }
    }
}
