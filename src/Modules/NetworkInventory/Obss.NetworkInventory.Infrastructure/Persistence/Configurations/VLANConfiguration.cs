using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.NetworkInventory.Infrastructure.Persistence.Configurations;

public sealed class VLANConfiguration : IEntityTypeConfiguration<VLAN>
{
    public void Configure(EntityTypeBuilder<VLAN> builder)
    {
        builder.ToTable("vlans");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .ValueGeneratedNever();

        builder.Property(v => v.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion<TenantIdValueConverter>()
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(v => v.VLANId)
            .HasColumnName("vlan_id")
            .IsRequired();

        builder.Property(v => v.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(v => v.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(v => v.Location)
            .HasColumnName("location")
            .HasMaxLength(500);

        builder.Property(v => v.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(v => v.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(v => v.VLANId)
            .HasDatabaseName("ix_vlans_vlan_id");

        builder.HasIndex(v => new { v.TenantId, v.VLANId })
            .HasDatabaseName("ix_vlans_tenant_id_vlan_id")
            .IsUnique();
    }
}
