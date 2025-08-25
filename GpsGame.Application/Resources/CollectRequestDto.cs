namespace GpsGame.Application.Resources;

public sealed class CollectRequestDto
{
    public Guid PlayerId { get; set; }
    public double PlayerLatitude { get; set; }
    public double PlayerLongitude { get; set; }
    public int Amount { get; set; } // [1..50]
}