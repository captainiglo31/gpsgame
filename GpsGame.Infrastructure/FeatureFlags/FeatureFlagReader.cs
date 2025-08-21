using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GpsGame.Application.FeatureFlags;
using GpsGame.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GpsGame.Infrastructure.FeatureFlags
{
    /// <summary>
    /// EF Core-based read-only implementation of <see cref="IFeatureFlagReader"/>.
    /// </summary>
    internal sealed class FeatureFlagReader : IFeatureFlagReader
    {
        private readonly AppDbContext _db;

        public FeatureFlagReader(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        /// <inheritdoc />
        public async Task<bool?> IsEnabledAsync(string key, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            var normalized = key.Trim();

            // Select directly to nullable bool to avoid loading entity.
            return await _db.FeatureFlags
                .AsNoTracking()
                .Where(f => f.Key == normalized)
                .Select(f => (bool?)f.Enabled)
                .FirstOrDefaultAsync(ct);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<(string Key, bool Enabled)>> GetAllAsync(CancellationToken ct = default)
        {
            var list = await _db.FeatureFlags
                .AsNoTracking()
                .OrderBy(f => f.Key)
                .Select(f => new ValueTuple<string, bool>(f.Key, f.Enabled))
                .ToListAsync(ct);

            // List<ValueTuple<,>> is already IReadOnlyList<(string,bool)>, no extra copy needed.
            return list;
        }
    }
}