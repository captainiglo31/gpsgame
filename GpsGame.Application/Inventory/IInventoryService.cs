// GpsGame.Application/Inventory/IInventoryService.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GpsGame.Application.Inventory
{
    /// <summary>Operations to modify and read player inventory.</summary>
    public interface IInventoryService
    {
        Task AddAsync(Guid playerId, string resourceType, long amount, CancellationToken ct = default);
        Task AddRangeAsync(Guid playerId, IEnumerable<(string resourceType, long amount)> items, CancellationToken ct = default);
        Task IncrementAsync(Guid playerId, string resourceType, long delta, CancellationToken ct = default);
        Task<IReadOnlyList<(string ResourceType, long Amount)>> GetByPlayerAsync(Guid playerId, CancellationToken ct = default);
    }
}