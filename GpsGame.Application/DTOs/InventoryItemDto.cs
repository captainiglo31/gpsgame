namespace GpsGame.Application.DTOs
{
    /// <summary>
    /// Aggregated inventory entry (grouped by resource type).
    /// </summary>
    public sealed record InventoryItemDto
    {
        /// <summary>Resource type key (e.g., "Iron").</summary>
        public string ResourceType { get; init; } = default!;

        /// <summary>Total amount for this resource type.</summary>
        public long Amount { get; init; }
    }
}