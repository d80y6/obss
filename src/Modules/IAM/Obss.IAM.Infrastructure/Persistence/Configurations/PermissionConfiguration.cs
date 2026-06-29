using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.IAM.Domain.Entities;

namespace Obss.IAM.Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.Code)
            .HasColumnName("code")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(p => p.Module)
            .HasColumnName("module")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Resource)
            .HasColumnName("resource")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Action)
            .HasColumnName("action")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(p => p.Code)
            .HasDatabaseName("ix_permissions_code")
            .IsUnique();
    }
}
