using GpsGame.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GpsGame.Infrastructure.Persistence.Configurations
{
    internal class FeatureFlagConfiguration : IEntityTypeConfiguration<FeatureFlag>
    {
        public void Configure(EntityTypeBuilder<FeatureFlag> builder)
        {
            builder.ToTable("FeatureFlags");

            builder.HasKey(ff => ff.Id);

            builder.Property(ff => ff.Key)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(ff => ff.Key)
                .IsUnique();

            builder.Property(ff => ff.Enabled)
                .IsRequired();

            builder.Property(ff => ff.CreatedUtc)
                .IsRequired();

            builder.Property(ff => ff.UpdatedUtc)
                .IsRequired();
        }
    }
}