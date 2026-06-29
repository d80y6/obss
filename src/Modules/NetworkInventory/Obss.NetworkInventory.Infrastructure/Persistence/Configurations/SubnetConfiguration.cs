using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.NetworkInventory.Infrastructure.Persistence.Configurations;

public sealed class SubnetConfiguration : IEntityTypeConfiguration<Subnet>
{
    public void Configure(EntityTypeBuilder<Subnet> builder)
    {
        builder.ToTable("subnets");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion<TenantIdValueConverter>()
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Network)
            .HasColumnName("network")
            .HasMaxLength(45)
            .IsRequired();

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(s => s.Gateway)
            .HasColumnName("gateway")
            .HasMaxLength(45);

        builder.Property(s => s.VLANId)
            .HasColumnName("vlan_id");

        builder.Property(s => s.Location)
            .HasColumnName("location")
            .HasMaxLength(500);

        builder.Property(s => s.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(s => s.Network)
            .HasDatabaseName("ix_subnets_network");

        builder.HasIndex(s => new { s.TenantId, s.Network })
            .HasDatabaseName("ix_subnets_tenant_id_network")
            .IsUnique();
    }
}
