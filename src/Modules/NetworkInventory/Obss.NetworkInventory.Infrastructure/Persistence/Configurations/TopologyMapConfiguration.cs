using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.NetworkInventory.Domain.Entities;

namespace Obss.NetworkInventory.Infrastructure.Persistence.Configurations;

public sealed class TopologyMapConfiguration : IEntityTypeConfiguration<TopologyMap>
{
    public void Configure(EntityTypeBuilder<TopologyMap> builder)
    {
        builder.ToTable("topology_maps");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .ValueGeneratedNever();

        builder.Property(m => m.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(m => m.Configuration)
            .HasColumnName("configuration")
            .HasColumnType("jsonb");

        builder.Property(m => m.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}
