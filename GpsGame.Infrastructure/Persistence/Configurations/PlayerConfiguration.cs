using GpsGame.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GpsGame.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// EF Core configuration for the Player entity.
    /// </summary>
    public sealed class PlayerConfiguration : IEntityTypeConfiguration<Player>
    {
        public void Configure(EntityTypeBuilder<Player> b)
        {
            b.ToTable("Players");

            // Key
            b.HasKey(p => p.Id);

            // Properties
            b.Property(p => p.Username)
                .IsRequired()
                .HasMaxLength(64);

            b.Property(p => p.ApiToken)
                .HasMaxLength(64);

            b.Property(p => p.Latitude)
                .IsRequired();

            b.Property(p => p.Longitude)
                .IsRequired();

            b.Property(p => p.CreatedUtc)
                .IsRequired();

            b.Property(p => p.UpdatedUtc)
                .IsRequired();

            // Indexes
            // Multiple NULLs are allowed by most providers; non-null tokens must be unique.
            b.HasIndex(p => p.ApiToken)
                .IsUnique();
        }
    }
}