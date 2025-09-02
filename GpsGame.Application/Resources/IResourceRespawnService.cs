using System.Threading;
using System.Threading.Tasks;

namespace GpsGame.Application.Resources
{
    /// <summary>
    /// Handles refilling resource nodes whose cooldown has expired.
    /// </summary>
    public interface IResourceRespawnService
    {
        /// <summary>
        /// Refills all due nodes within the given bounding box.
        /// Returns the number of updated rows.
        /// </summary>
        Task<int> RespawnDueAsync(
            double minLat, double minLng, double maxLat, double maxLng,
            CancellationToken ct);

        /// <summary>
        /// Refills a single node if its cooldown has expired.
        /// Returns true if it was updated.
        /// </summary>
        Task<bool> RespawnIfDueAsync(
            System.Guid nodeId,
            CancellationToken ct);
    }
}