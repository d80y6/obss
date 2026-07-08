using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Provisioning.Domain.Entities;

namespace Obss.Provisioning.Infrastructure.Persistence.Configurations;

public sealed class ServiceOrderItemConfiguration : IEntityTypeConfiguration<ServiceOrderItem>
{
    public void Configure(EntityTypeBuilder<ServiceOrderItem> builder)
    {
        builder.ToTable("service_order_items");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(i => i.ServiceOrderId)
            .HasColumnName("service_order_id")
            .IsRequired();

        builder.Property(i => i.ServiceId)
            .HasColumnName("service_id");

        builder.Property(i => i.Action)
            .HasColumnName("action")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(i => i.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(i => i.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(i => i.State)
            .HasColumnName("state")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(i => i.RequestedStartDate)
            .HasColumnName("requested_start_date");

        builder.Property(i => i.RequestedCompletionDate)
            .HasColumnName("requested_completion_date");

        builder.Property(i => i.CompletedDate)
            .HasColumnName("completed_date");

        builder.Property(i => i.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(2000);

        builder.HasIndex(i => i.ServiceOrderId)
            .HasDatabaseName("ix_service_order_items_service_order_id");
    }
}