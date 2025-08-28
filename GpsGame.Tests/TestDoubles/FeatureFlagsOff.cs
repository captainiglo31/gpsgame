using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GpsGame.Application.FeatureFlags;

namespace GpsGame.Tests.TestDoubles
{
    /// <summary>Feature-Flags-Fake, der immer "aus" zurückgibt.</summary>
    public sealed class FeatureFlagsOff : IFeatureFlagReader
    {
        public Task<bool?> IsEnabledAsync(string key, CancellationToken ct = default)
            => Task.FromResult<bool?>(false);

        public Task<IReadOnlyList<(string Key, bool Enabled)>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<(string Key, bool Enabled)>>(new List<(string, bool)>());
    }
}