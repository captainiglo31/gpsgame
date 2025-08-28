using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GpsGame.Application.FeatureFlags;

namespace GpsGame.Tests.TestDoubles
{
    public sealed class AlwaysOnFeatureFlags : IFeatureFlagReader
    {
        public Task<bool?> IsEnabledAsync(string key, CancellationToken ct = default)
            => Task.FromResult<bool?>(true);

        public Task<IReadOnlyList<(string Key, bool Enabled)>> GetAllAsync(CancellationToken ct = default)
        {
            IReadOnlyList<(string Key, bool Enabled)> all = new List<(string Key, bool Enabled)>
            {
                (Key: "*", Enabled: true) // optional: „alles an“
            };
            return Task.FromResult(all);
        }
    }
}