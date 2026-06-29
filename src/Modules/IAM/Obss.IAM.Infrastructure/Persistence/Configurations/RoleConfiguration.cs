using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.IAM.Domain.Entities;

namespace Obss.IAM.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(r => r.IsSystem)
            .HasColumnName("is_system")
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasMany(r => r.Permissions)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "role_permissions",
                    j => j.HasOne<Permission>().WithMany().HasForeignKey("permission_id"),
                    j => j.HasOne<Role>().WithMany().HasForeignKey("role_id"),
                    j =>
                    {
                        j.ToTable("role_permissions");
                        j.HasKey("role_id", "permission_id");
                    });

        builder.HasIndex(r => new { r.TenantId, r.Name })
            .HasDatabaseName("ix_roles_tenant_id_name")
            .IsUnique();
    }
}
