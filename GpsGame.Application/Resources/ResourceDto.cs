using System;

namespace GpsGame.Application.Resources
{
    /// <summary>
    /// Data transfer object for a resource node.
    /// </summary>
    public class ResourceDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = default!;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Amount { get; set; }
        public int MaxAmount { get; set; }
        public DateTime? RespawnAtUtc { get; set; }
    }
}