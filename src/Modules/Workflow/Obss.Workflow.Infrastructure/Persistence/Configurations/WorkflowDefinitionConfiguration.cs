using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Workflow.Domain.Entities;

namespace Obss.Workflow.Infrastructure.Persistence.Configurations;

public sealed class WorkflowDefinitionConfiguration : IEntityTypeConfiguration<WorkflowDefinition>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
    {
        builder.ToTable("workflow_definitions");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .ValueGeneratedNever();

        builder.Property(w => w.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(w => w.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(w => w.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(w => w.Category)
            .HasColumnName("category")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(w => w.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(w => w.Version)
            .HasColumnName("version")
            .IsRequired();

        builder.Property(w => w.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(w => w.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasMany(w => w.Steps)
            .WithOne(s => s.WorkflowDefinition)
            .HasForeignKey(s => s.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => new { w.TenantId, w.Name, w.Version })
            .HasDatabaseName("ix_workflow_definitions_tenant_name_version")
            .IsUnique();

        builder.Navigation(w => w.Steps)
            .AutoInclude();
    }
}
