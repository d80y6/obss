using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Payments.Domain.Entities;

namespace Obss.Payments.Infrastructure.Persistence.Configurations;

public sealed class PaymentAllocationConfiguration : IEntityTypeConfiguration<PaymentAllocation>
{
    public void Configure(EntityTypeBuilder<PaymentAllocation> builder)
    {
        builder.ToTable("payment_allocations");

        builder.HasKey(pa => pa.Id);

        builder.Property(pa => pa.Id)
            .ValueGeneratedNever();

        builder.Property(pa => pa.PaymentId)
            .HasColumnName("payment_id")
            .IsRequired();

        builder.Property(pa => pa.InvoiceId)
            .HasColumnName("invoice_id")
            .IsRequired();

        builder.Property(pa => pa.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(100);

        builder.Property(pa => pa.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(pa => pa.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(pa => pa.PaymentId)
            .HasDatabaseName("ix_payment_allocations_payment_id");

        builder.HasIndex(pa => pa.InvoiceId)
            .HasDatabaseName("ix_payment_allocations_invoice_id");
    }
}
