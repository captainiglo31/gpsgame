using GpsGame.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GpsGame.Infrastructure.Persistence.Configurations;

public sealed class PlayerResourceCollectConfiguration : IEntityTypeConfiguration<PlayerResourceCollect>
{
    public void Configure(EntityTypeBuilder<PlayerResourceCollect> b)
    {
        b.ToTable("PlayerResourceCollect");
        b.HasKey(x => x.Id);
        b.Property(x => x.CreatedUtc).IsRequired();
        b.HasIndex(x => new { x.PlayerId, x.ResourceNodeId, x.CreatedUtc })
            .HasDatabaseName("IX_Collect_Player_Node_CreatedUtc");
    }
}