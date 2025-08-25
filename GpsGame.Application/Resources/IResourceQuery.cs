using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GpsGame.Application.Resources
{
    /// <summary>
    /// Provides query operations for resource nodes.
    /// </summary>
    public interface IResourceQuery
    {
        /// <summary>
        /// Gets resource nodes within the specified bounding box.
        /// </summary>
        /// <param name="minLat">Minimum latitude.</param>
        /// <param name="minLng">Minimum longitude.</param>
        /// <param name="maxLat">Maximum latitude.</param>
        /// <param name="maxLng">Maximum longitude.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<IReadOnlyList<ResourceDto>> GetByBoundingBoxAsync(
            double minLat,
            double minLng,
            double maxLat,
            double maxLng,
            CancellationToken ct);
    }
}
