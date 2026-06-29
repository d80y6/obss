using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.IAM.Domain.Entities;

namespace Obss.IAM.Infrastructure.Persistence.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Slug)
            .HasColumnName("slug")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.ConnectionString)
            .HasColumnName("connection_string")
            .HasMaxLength(500);

        builder.Property(t => t.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(t => t.Settings)
            .HasColumnName("settings")
            .HasColumnType("jsonb");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(t => t.Slug)
            .HasDatabaseName("ix_tenants_slug")
            .IsUnique();
    }
}
