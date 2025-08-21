using System;
using System.ComponentModel.DataAnnotations;

namespace GpsGame.Domain.Entities
{
    public class FeatureFlag
    {
        public Guid Id { get; set; }

        [MaxLength(100)]
        public string Key { get; set; } = null!;

        public bool Enabled { get; set; }

        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
    }
}