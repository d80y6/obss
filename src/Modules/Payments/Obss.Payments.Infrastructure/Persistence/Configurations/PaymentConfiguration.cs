using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Payments.Domain.Entities;

namespace Obss.Payments.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.PaymentNumber)
            .HasColumnName("payment_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(p => p.InvoiceId)
            .HasColumnName("invoice_id");

        builder.Property(p => p.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(p => p.PaymentMethod)
            .HasColumnName("payment_method")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.PaymentReference)
            .HasColumnName("payment_reference")
            .HasMaxLength(200);

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.PaidAt)
            .HasColumnName("paid_at")
            .IsRequired();

        builder.Property(p => p.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(p => p.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasMany(p => p.Allocations)
            .WithOne()
            .HasForeignKey(a => a.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Refunds)
            .WithOne()
            .HasForeignKey(r => r.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.PaymentNumber)
            .HasDatabaseName("ix_payments_payment_number")
            .IsUnique();

        builder.HasIndex(p => p.CustomerId)
            .HasDatabaseName("ix_payments_customer_id");

        builder.HasIndex(p => p.InvoiceId)
            .HasDatabaseName("ix_payments_invoice_id");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("ix_payments_status");

        builder.HasIndex(p => new { p.TenantId, p.PaymentNumber })
            .HasDatabaseName("ix_payments_tenant_id_payment_number")
            .IsUnique();

        builder.Navigation(p => p.Allocations)
            .AutoInclude();

        builder.Navigation(p => p.Refunds)
            .AutoInclude();
    }
}
