using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Provisioning.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Provisioning.Infrastructure.Persistence.Configurations;

public sealed class ServiceOrderConfiguration : IEntityTypeConfiguration<ServiceOrder>
{
    public void Configure(EntityTypeBuilder<ServiceOrder> builder)
    {
        builder.ToTable("service_orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .ValueGeneratedNever();

        builder.Property(o => o.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(o => o.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(100);

        builder.Property(o => o.State)
            .HasColumnName("state")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(o => o.Priority)
            .HasColumnName("priority")
            .HasMaxLength(50);

        builder.Property(o => o.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(o => o.Category)
            .HasColumnName("category")
            .HasMaxLength(100);

        builder.Property(o => o.RequestedStartDate)
            .HasColumnName("requested_start_date");

        builder.Property(o => o.RequestedCompletionDate)
            .HasColumnName("requested_completion_date");

        builder.Property(o => o.OrderDate)
            .HasColumnName("order_date")
            .IsRequired();

        builder.Property(o => o.StatusChangeDate)
            .HasColumnName("status_change_date");

        builder.Property(o => o.CompletionMessage)
            .HasColumnName("completion_message")
            .HasMaxLength(2000);

        builder.Property(o => o.Href)
            .HasColumnName("href")
            .HasMaxLength(500);

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.ServiceOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.RelatedParties)
            .WithOne()
            .HasForeignKey("ServiceOrderId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Characteristics)
            .WithOne()
            .HasForeignKey("ServiceOrderId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Milestones)
            .WithOne()
            .HasForeignKey("ServiceOrderId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Notes)
            .WithOne()
            .HasForeignKey("ServiceOrderId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(o => o.CancelRequest, cancel =>
        {
            cancel.WithOwner();
            cancel.Property(c => c.Id).HasColumnName("cancel_id");
            cancel.Property(c => c.Reason).HasColumnName("cancel_reason").HasMaxLength(1000);
            cancel.Property(c => c.CompletedDate).HasColumnName("cancel_completed_date");
            cancel.Property(c => c.State).HasColumnName("cancel_state").HasMaxLength(50);
        });

        builder.HasIndex(o => o.TenantId)
            .HasDatabaseName("ix_service_orders_tenant_id");

        builder.HasIndex(o => o.State)
            .HasDatabaseName("ix_service_orders_state");

        builder.HasIndex(o => o.ExternalId)
            .HasDatabaseName("ix_service_orders_external_id");

        builder.Navigation(o => o.Items)
            .AutoInclude();
    }
}
