using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Provisioning.Domain.Entities;

namespace Obss.Provisioning.Infrastructure.Persistence.Configurations;

public sealed class ProvisioningTemplateConfiguration : IEntityTypeConfiguration<ProvisioningTemplate>
{
    public void Configure(EntityTypeBuilder<ProvisioningTemplate> builder)
    {
        builder.ToTable("provisioning_templates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.Property(t => t.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(t => t.ServiceType)
            .HasColumnName("service_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.Action)
            .HasColumnName("action")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.WorkflowDefinitionId)
            .HasColumnName("workflow_definition_id")
            .IsRequired();

        builder.Property(t => t.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(t => new { t.TenantId, t.ServiceType, t.Action })
            .HasDatabaseName("ix_provisioning_templates_tenant_service_action");
    }
}
