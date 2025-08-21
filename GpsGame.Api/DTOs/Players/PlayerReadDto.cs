namespace GpsGame.Api.DTOs.Players;

public class PlayerReadDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime CreatedUtc { get; set; }
}