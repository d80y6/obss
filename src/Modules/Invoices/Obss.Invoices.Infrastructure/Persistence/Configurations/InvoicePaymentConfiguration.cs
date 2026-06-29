using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Invoices.Domain.Entities;

namespace Obss.Invoices.Infrastructure.Persistence.Configurations;

public sealed class InvoicePaymentConfiguration : IEntityTypeConfiguration<InvoicePayment>
{
    public void Configure(EntityTypeBuilder<InvoicePayment> builder)
    {
        builder.ToTable("invoice_payments");

        builder.HasKey(ip => ip.Id);

        builder.Property(ip => ip.Id)
            .ValueGeneratedNever();

        builder.Property(ip => ip.InvoiceId)
            .HasColumnName("invoice_id")
            .IsRequired();

        builder.Property(ip => ip.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(ip => ip.PaymentReference)
            .HasColumnName("payment_reference")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(ip => ip.PaidAt)
            .HasColumnName("paid_at")
            .IsRequired();

        builder.HasIndex(ip => ip.InvoiceId)
            .HasDatabaseName("ix_invoice_payments_invoice_id");

        builder.HasIndex(ip => ip.PaymentReference)
            .HasDatabaseName("ix_invoice_payments_payment_reference");
    }
}
