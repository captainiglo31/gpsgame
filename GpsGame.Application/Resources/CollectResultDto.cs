namespace GpsGame.Application.Resources;

public sealed class CollectResultDto
{
    public bool Success { get; init; }
    public string? Reason { get; init; } // "disabled" | "not_found" | "respawning" | "too_far" | "depleted_or_race"
    public int Collected { get; init; }
    public int Remaining { get; init; }
    public DateTime? RespawnAtUtc { get; init; }
    public Guid PlayerId { get; init; }
    public string ResourceType { get; init; } = "";
}