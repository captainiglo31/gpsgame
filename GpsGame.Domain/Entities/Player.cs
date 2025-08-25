namespace GpsGame.Domain.Entities;

public class Player
{
    public Guid Id { get; set; }
    public string? Username { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? ApiToken { get; set; } // random string / GUID
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
}