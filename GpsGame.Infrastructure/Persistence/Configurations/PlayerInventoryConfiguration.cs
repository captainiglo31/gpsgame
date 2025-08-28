// GpsGame.Infrastructure/Persistence/Configurations/PlayerInventoryConfiguration.cs
using GpsGame.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GpsGame.Infrastructure.Persistence.Configurations
{
    public class PlayerInventoryConfiguration : IEntityTypeConfiguration<PlayerInventory>
    {
        public void Configure(EntityTypeBuilder<PlayerInventory> b)
        {
            b.ToTable("PlayerInventory");
            b.HasKey(x => x.Id);

            b.Property(x => x.PlayerId).IsRequired();
            b.Property(x => x.ResourceType).HasMaxLength(64).IsRequired();
            b.Property(x => x.Amount).IsRequired();
            b.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone").IsRequired();

            b.HasIndex(x => new { x.PlayerId, x.ResourceType }).IsUnique();

            b.HasOne(x => x.Player)
                .WithMany()
                .HasForeignKey(x => x.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}