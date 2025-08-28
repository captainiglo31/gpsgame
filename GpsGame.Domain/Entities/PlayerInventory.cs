// GpsGame.Domain/Entities/PlayerInventory.cs
using System;

namespace GpsGame.Domain.Entities
{
    /// <summary>Persistent inventory per player and resource type.</summary>
    public class PlayerInventory
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; }
        public string ResourceType { get; set; } = default!;
        public long Amount { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Player? Player { get; set; }
    }
}