using GpsGame.Application.FeatureFlags;
using GpsGame.Application.Resources;
using GpsGame.Domain.Entities;
using GpsGame.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GpsGame.Application.Security;

namespace GpsGame.Infrastructure.Services;

public sealed class ResourceCollector : IResourceCollector
{
    private static readonly TimeSpan Cooldown = TimeSpan.FromSeconds(3);
    
    private const double MaxDistanceMeters = 50.0;
    
    private readonly AppDbContext _db;
    private readonly IFeatureFlagReader _flags;
    private readonly ILogger<ResourceCollector> _logger;
    private readonly ICurrentPlayerAccessor _current;
    private readonly IResourceRules _rules;
    
    
    
    

    public ResourceCollector(AppDbContext db, IFeatureFlagReader flags, ILogger<ResourceCollector> logger, ICurrentPlayerAccessor current, IResourceRules rules)
    {
        _db = db;
        _flags = flags;
        _logger = logger;
        _current = current;
        _rules = rules;
    }

    public async Task<CollectResultDto> CollectAsync(Guid nodeId, CollectRequestDto request, CancellationToken ct = default)
    {
        var playerId = _current.PlayerId;
        if (playerId is null)
        {
            _logger.LogWarning("Collect unauthorized: no player in context. node={NodeId}", nodeId);
            return new CollectResultDto { Success = false, Reason = "unauthorized" };
        }
        
        // Feature gate
        if (!(await _flags.IsEnabledAsync("resources_enabled", ct) ?? false))
        {
           _logger.LogWarning("Collect blocked: feature 'resources_enabled' disabled. player={PlayerId}, node={NodeId}",
               request.PlayerId, nodeId);
            return new CollectResultDto { Success = false, Reason = "disabled" };
        }

        var nowUtc = DateTime.UtcNow;

        var node = await _db.ResourceNodes
            .AsNoTracking()
            .Where(n => n.Id == nodeId)
            .Select(n => new { n.Id, n.Latitude, n.Longitude, n.Amount, n.RespawnAtUtc,  n.Type })
            .FirstOrDefaultAsync(ct);

        if (node is null)
        {
           _logger.LogWarning("Collect failed: node not found. player={PlayerId}, node={NodeId}",
               request.PlayerId, nodeId);
            return new CollectResultDto { Success = false, Reason = "not_found" };
        }

        // Cooldown (Phase B) – falls bereits eingebaut
       var since = nowUtc - Cooldown;
       var recentHit = await _db.PlayerResourceCollects
           .AsNoTracking()
           .Where(c => c.PlayerId == request.PlayerId && c.ResourceNodeId == nodeId && c.CreatedUtc >= since)
           .OrderByDescending(c => c.CreatedUtc)
           .FirstOrDefaultAsync(ct);
       
       if (recentHit is not null)
       {
           _logger.LogWarning("Collect blocked: cooldown. player={PlayerId}, node={NodeId}, lastHitUtc={LastHitUtc}, sinceUtc={SinceUtc}",
               request.PlayerId, nodeId, recentHit.CreatedUtc, since);
           return new CollectResultDto
           {
               Success = false, Reason = "cooldown",
               Remaining = node.Amount, RespawnAtUtc = node.RespawnAtUtc,
               PlayerId = request.PlayerId,                 
               ResourceType = node.Type,
               CollectedNodeId = node.Id
           };
       }
       
       // Log-Eintrag nach Erfolg:
       _db.PlayerResourceCollects.Add(new PlayerResourceCollect
       {
           Id = Guid.NewGuid(),
           PlayerId = request.PlayerId,
           ResourceNodeId = nodeId,
           CreatedUtc = nowUtc
       });
       await _db.SaveChangesAsync(ct);

        // Respawn check
        if (node.RespawnAtUtc.HasValue && node.RespawnAtUtc.Value > nowUtc)
        {
           _logger.LogWarning("Collect blocked: respawning. player={PlayerId}, node={NodeId}, respawnAtUtc={RespawnAtUtc}, amount={Amount}",
               request.PlayerId, nodeId, node.RespawnAtUtc, node.Amount);
            return new CollectResultDto
            {
                Success = false, 
                Reason = "respawning", 
                Remaining = node.Amount, 
                RespawnAtUtc = node.RespawnAtUtc, 
                PlayerId = new Guid(playerId.ToString()), 
                ResourceType = node.Type,
                CollectedNodeId = node.Id,
            };
        }

        // Distance check
        var distance = HaversineMeters(request.PlayerLatitude, request.PlayerLongitude, node.Latitude, node.Longitude);
        if (distance > MaxDistanceMeters)
        {
           _logger.LogWarning("Collect blocked: too far. player={PlayerId}, node={NodeId}, distanceMeters={Distance:F1}, maxMeters={Max}",
               request.PlayerId, nodeId, distance, MaxDistanceMeters);
            return new CollectResultDto { Success = false, Reason = "too_far" };
        }

        var effective = Math.Min(request.Amount, node.Amount);
        if (effective <= 0)
        {
           _logger.LogWarning("Collect failed: depleted or race (pre-check). player={PlayerId}, node={NodeId}, nodeAmount={Amount}",
               request.PlayerId, nodeId, node.Amount);
            return new CollectResultDto
            {
                Success = false, 
                Reason = "depleted_or_race", 
                Remaining = node.Amount, 
                PlayerId = new Guid(playerId.ToString()), 
                ResourceType = node.Type,
                CollectedNodeId = node.Id
            };
        }

        var rows = await _db.ResourceNodes
            .Where(n => n.Id == nodeId
                        && n.Amount > 0
                        && (!n.RespawnAtUtc.HasValue || n.RespawnAtUtc <= nowUtc)
                        && n.Amount >= effective)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(n => n.Amount, n => n.Amount - effective)
                .SetProperty(n => n.UpdatedUtc, _ => nowUtc), ct);

        if (rows == 0)
        {
           _logger.LogWarning("Collect failed: depleted_or_race (atomic update miss). player={PlayerId}, node={NodeId}, requested={Requested}",
               request.PlayerId, nodeId, effective);
            return new CollectResultDto { Success = false, Reason = "depleted_or_race" };
        }

        // ggf. Respawn setzen …
        var respawnMinutes = _rules.GetRespawnMinutes(node.Type ?? "");
        var respawnAt = nowUtc.AddMinutes(respawnMinutes);
        await _db.ResourceNodes
            .Where(n => n.Id == nodeId && n.Amount == 0 && (!n.RespawnAtUtc.HasValue || n.RespawnAtUtc <= nowUtc))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(n => n.RespawnAtUtc, _ => respawnAt)
                .SetProperty(n => n.UpdatedUtc, _ => nowUtc), ct);
      

        var after = await _db.ResourceNodes
            .AsNoTracking()
            .Where(n => n.Id == nodeId)
            .Select(n => new { n.Amount, n.RespawnAtUtc })
            .FirstAsync(ct);

       _logger.LogInformation("Collect success. player={PlayerId}, node={NodeId}, collected={Collected}, remaining={Remaining}, respawnAtUtc={RespawnAtUtc}",
           request.PlayerId, nodeId, effective, after.Amount, after.Amount == 0 ? after.RespawnAtUtc : null);
       
        return new CollectResultDto
        {
            Success = true,
            Collected = effective,
            Remaining = after.Amount,
            RespawnAtUtc = after.Amount == 0 ? after.RespawnAtUtc : null,
            PlayerId = new Guid(playerId.ToString()),
            ResourceType = node.Type,
            CollectedNodeId = node.Id
            
            
        };
    }

    private static double HaversineMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000.0; // meters
        double dLat = DegreesToRadians(lat2 - lat1);
        double dLon = DegreesToRadians(lon2 - lon1);
        double a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double DegreesToRadians(double deg) => deg * (Math.PI / 180.0);
}
