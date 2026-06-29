using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Provisioning.Domain.Entities;

namespace Obss.Provisioning.Infrastructure.Persistence.Configurations;

public sealed class ProvisioningJobConfiguration : IEntityTypeConfiguration<ProvisioningJob>
{
    public void Configure(EntityTypeBuilder<ProvisioningJob> builder)
    {
        builder.ToTable("provisioning_jobs");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Id)
            .ValueGeneratedNever();

        builder.Property(j => j.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(j => j.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(j => j.OrderItemId)
            .HasColumnName("order_item_id")
            .IsRequired();

        builder.Property(j => j.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(j => j.ServiceId)
            .HasColumnName("service_id");

        builder.Property(j => j.ServiceType)
            .HasColumnName("service_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(j => j.Action)
            .HasColumnName("action")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(j => j.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(j => j.WorkflowInstanceId)
            .HasColumnName("workflow_instance_id");

        builder.Property(j => j.StartedAt)
            .HasColumnName("started_at");

        builder.Property(j => j.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(j => j.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(2000);

        builder.Property(j => j.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(j => j.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasMany(j => j.Tasks)
            .WithOne()
            .HasForeignKey(t => t.ProvisioningJobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(j => j.Tasks)
            .AutoInclude();

        builder.HasIndex(j => j.OrderId)
            .HasDatabaseName("ix_provisioning_jobs_order_id");

        builder.HasIndex(j => j.Status)
            .HasDatabaseName("ix_provisioning_jobs_status");

        builder.HasIndex(j => j.ServiceId)
            .HasDatabaseName("ix_provisioning_jobs_service_id");
    }
}
