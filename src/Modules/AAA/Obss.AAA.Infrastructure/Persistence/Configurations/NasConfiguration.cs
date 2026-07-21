using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.AAA.Domain.Entities;

namespace Obss.AAA.Infrastructure.Persistence.Configurations;

public sealed class NasConfiguration : IEntityTypeConfiguration<NetworkAccessServer>
{
    public void Configure(EntityTypeBuilder<NetworkAccessServer> builder)
    {
        builder.ToTable("nas_devices");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id).ValueGeneratedNever();

        builder.Property(n => n.TenantId).HasColumnName("tenant_id").HasMaxLength(100).IsRequired();
        builder.Property(n => n.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(n => n.NasIpAddress).HasColumnName("nas_ip_address").HasMaxLength(45).IsRequired();
        builder.Property(n => n.NasSecret).HasColumnName("nas_secret").HasMaxLength(500).IsRequired();
        builder.Property(n => n.NasType).HasColumnName("nas_type").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(n => n.Location).HasColumnName("location").HasMaxLength(500);
        builder.Property(n => n.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(n => n.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(n => n.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(n => n.NasIpAddress).HasDatabaseName("ix_nas_devices_ip");
        builder.HasIndex(n => new { n.TenantId, n.Name }).IsUnique().HasDatabaseName("ix_nas_devices_tenant_name");
    }
}
