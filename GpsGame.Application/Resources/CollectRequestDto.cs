using System.ComponentModel.DataAnnotations;

namespace GpsGame.Application.Resources;

public sealed class CollectRequestDto
{
    public Guid PlayerId { get; set; }   
    public double PlayerLatitude { get; set; }
    public double PlayerLongitude { get; set; }
    [Range(1, 50)]
    public int Amount { get; set; }
}
