namespace GpsGame.Domain.Entities;

public class Player
{
    public Guid Id { get; set; }
    public string? Username { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}