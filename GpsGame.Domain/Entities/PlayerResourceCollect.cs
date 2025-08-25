namespace GpsGame.Domain.Entities;

public sealed class PlayerResourceCollect
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Guid ResourceNodeId { get; set; }
    public DateTime CreatedUtc { get; set; }
}