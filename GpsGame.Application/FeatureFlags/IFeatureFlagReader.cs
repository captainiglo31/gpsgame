using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GpsGame.Application.FeatureFlags
{
    /// <summary>
    /// Provides read-only access to feature flags.
    /// </summary>
    public interface IFeatureFlagReader
    {
        /// <summary>
        /// Returns whether a feature flag is enabled.
        /// </summary>
        /// <param name="key">Unique feature flag key (case-sensitive unless configured otherwise in DB).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>
        /// <c>true</c> if enabled; <c>false</c> if disabled; <c>null</c> if the key does not exist or input is empty.
        /// </returns>
        Task<bool?> IsEnabledAsync(string key, CancellationToken ct = default);

        /// <summary>
        /// Returns all feature flags as an immutable list of (Key, Enabled) tuples.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        Task<IReadOnlyList<(string Key, bool Enabled)>> GetAllAsync(CancellationToken ct = default);
    }
}