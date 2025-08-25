using System;

namespace GpsGame.Domain.Entities
{
    /// <summary>
    /// Represents a resource node in the game world.
    /// </summary>
    public class ResourceNode
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Type of the resource (e.g., "iron", "wood", "stone").
        /// </summary>
        public string Type { get; set; } = default!;

        /// <summary>
        /// Latitude coordinate.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude coordinate.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Current available amount.
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// Maximum possible amount.
        /// </summary>
        public int MaxAmount { get; set; }

        /// <summary>
        /// Created timestamp (UTC).
        /// </summary>
        public DateTime CreatedUtc { get; set; }

        /// <summary>
        /// Last updated timestamp (UTC).
        /// </summary>
        public DateTime UpdatedUtc { get; set; }

        /// <summary>
        /// Scheduled respawn time (UTC). Null if no respawn planned.
        /// </summary>
        public DateTime? RespawnAtUtc { get; set; }
    }
}