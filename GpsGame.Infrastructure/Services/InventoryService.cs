// GpsGame.Infrastructure/Services/InventoryService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GpsGame.Application.DTOs;
using GpsGame.Application.Inventory;
using GpsGame.Domain.Entities;
using GpsGame.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GpsGame.Infrastructure.Services
{
    /// <summary>
    /// Direct-DbContext service (no repository).
    /// Ensures unique (PlayerId, ResourceType) row and atomic increments.
    /// </summary>
    public sealed class InventoryService : IInventoryService
    {
        private readonly AppDbContext _db;

        public InventoryService(AppDbContext db) => _db = db;

        public Task AddAsync(Guid playerId, string resourceType, long amount, CancellationToken ct = default)
            => IncrementAsync(playerId, resourceType, amount, ct);

        public async Task AddRangeAsync(Guid playerId, IEnumerable<(string resourceType, long amount)> items, CancellationToken ct = default)
        {
            foreach (var (type, amount) in items)
                await IncrementAsync(playerId, type, amount, ct);
        }

        public async Task IncrementAsync(Guid playerId, string resourceType, long amount, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var type = NormalizeType(resourceType);

            // Try tracked entity first (fast path inside same DbContext scope)
            var current = await _db.Set<PlayerInventory>()
                                   .FirstOrDefaultAsync(x => x.PlayerId == playerId && x.ResourceType == type, ct);

            if (current is null)
            {
                // create new
                var entity = new PlayerInventory
                {
                    Id = Guid.NewGuid(),
                    PlayerId = playerId,
                    ResourceType = resourceType,
                    Amount = amount,
                    UpdatedAt = now
                };

                _db.Add(entity);

                try
                {
                    await _db.SaveChangesAsync(ct);
                    return;
                }
                catch (DbUpdateException)
                {
                    // Unique-index race (another request created the row) -> reload + update
                    _db.ChangeTracker.Clear();
                    current = await _db.Set<PlayerInventory>()
                                       .FirstOrDefaultAsync(x => x.PlayerId == playerId && x.ResourceType == resourceType, ct);
                }
            }

            // update existing
            if (current is not null)
            {
                current.Amount += amount;
                current.UpdatedAt = now;
                _db.Update(current);
                await _db.SaveChangesAsync(ct);
            }
        }

        public async Task<IReadOnlyList<(string ResourceType, long Amount)>> GetByPlayerAsync(Guid playerId, CancellationToken ct = default)
        {
            var data = await _db.Set<PlayerInventory>()
                                .Where(x => x.PlayerId == playerId)
                                .GroupBy(x => x.ResourceType)
                                .Select(g => new { g.Key, Sum = g.Sum(x => x.Amount) })
                                .ToListAsync(ct);

            return data.Select(x => (x.Key, x.Sum)).ToList();
        }
        
        /// <inheritdoc />
        public async Task<IReadOnlyList<InventoryItemDto>> GetAggregatedByPlayerAsync(Guid playerId, CancellationToken ct)
        {
            // Groups inventory rows by ResourceType and sums Amount.
            var data = await _db.PlayerInventory
                .AsNoTracking()
                .Where(x => x.PlayerId == playerId)
                .GroupBy(x => x.ResourceType)
                .Select(g => new InventoryItemDto
                {
                    ResourceType = g.Key,
                    Amount = g.Sum(x => x.Amount)
                })
                .ToListAsync(ct);

            return data;
        }
        
        private static string NormalizeType(string resourceType)
            => resourceType?.Trim().ToUpperInvariant() ?? string.Empty;

    }
}
