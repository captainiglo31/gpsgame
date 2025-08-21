namespace GpsGame.Api.DTOs.Players;

public class PlayerUpdateDto
{
    public string Username { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}