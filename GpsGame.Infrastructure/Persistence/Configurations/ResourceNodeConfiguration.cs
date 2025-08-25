using GpsGame.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GpsGame.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Configures the ResourceNode entity.
    /// </summary>
    public class ResourceNodeConfiguration : IEntityTypeConfiguration<ResourceNode>
    {
        public void Configure(EntityTypeBuilder<ResourceNode> builder)
        {
            builder.ToTable("ResourceNodes");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Type)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(r => r.Latitude)
                .HasPrecision(9, 6)
                .IsRequired();

            builder.Property(r => r.Longitude)
                .HasPrecision(9, 6)
                .IsRequired();

            builder.Property(r => r.Amount)
                .IsRequired();

            builder.Property(r => r.MaxAmount)
                .IsRequired();

            builder.Property(r => r.CreatedUtc)
                .IsRequired();

            builder.Property(r => r.UpdatedUtc)
                .IsRequired();

            builder.Property(r => r.RespawnAtUtc);

            // index on coordinates for bounding box queries
            builder.HasIndex(r => new { r.Latitude, r.Longitude });
        }
    }
}