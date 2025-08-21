namespace GpsGame.Application.Players
{
    public class PlayerCreateDto
    {
        public string Username { get; set; } = null!;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}