using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Orders.Domain.Entities;

namespace Obss.Orders.Infrastructure.Persistence.Configurations;

public sealed class OrderFulfillmentConfiguration : IEntityTypeConfiguration<OrderFulfillment>
{
    public void Configure(EntityTypeBuilder<OrderFulfillment> builder)
    {
        builder.ToTable("order_fulfillments");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .ValueGeneratedNever();

        builder.Property(f => f.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(f => f.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(f => f.WorkflowInstanceId)
            .HasColumnName("workflow_instance_id");

        builder.Property(f => f.StartedAt)
            .HasColumnName("started_at")
            .IsRequired();

        builder.Property(f => f.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(f => f.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(2000);

        builder.HasIndex(f => f.OrderId)
            .HasDatabaseName("ix_order_fulfillments_order_id")
            .IsUnique();

        builder.HasIndex(f => f.Status)
            .HasDatabaseName("ix_order_fulfillments_status");
    }
}
