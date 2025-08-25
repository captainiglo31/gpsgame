using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GpsGame.Application.Resources;
using GpsGame.Infrastructure.Persistence;

namespace GpsGame.Infrastructure.Resources
{
    /// <summary>
    /// EF Core implementation of resource queries.
    /// </summary>
    public class ResourceQuery : IResourceQuery
    {
        private readonly AppDbContext _db;

        public ResourceQuery(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<ResourceDto>> GetByBoundingBoxAsync(
            double minLat,
            double minLng,
            double maxLat,
            double maxLng,
            CancellationToken ct)
        {
            return await _db.ResourceNodes
                .AsNoTracking()
                .Where(r => r.Latitude >= minLat && r.Latitude <= maxLat
                            && r.Longitude >= minLng && r.Longitude <= maxLng)
                .Select(r => new ResourceDto
                {
                    Id = r.Id,
                    Type = r.Type,
                    Latitude = r.Latitude,
                    Longitude = r.Longitude,
                    Amount = r.Amount,
                    MaxAmount = r.MaxAmount,
                    RespawnAtUtc = r.RespawnAtUtc
                })
                .ToListAsync(ct);
        }
    }
}