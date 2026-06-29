using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Payments.Domain.Entities;

namespace Obss.Payments.Infrastructure.Persistence.Configurations;

public sealed class ReconciliationItemConfiguration : IEntityTypeConfiguration<ReconciliationItem>
{
    public void Configure(EntityTypeBuilder<ReconciliationItem> builder)
    {
        builder.ToTable("reconciliation_items");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(i => i.ReconciliationId)
            .HasColumnName("reconciliation_id")
            .IsRequired();

        builder.Property(i => i.ExternalReference)
            .HasColumnName("external_reference")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(i => i.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(i => i.TransactionDate)
            .HasColumnName("transaction_date")
            .IsRequired();

        builder.Property(i => i.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(i => i.MatchedInvoiceId)
            .HasColumnName("matched_invoice_id");

        builder.Property(i => i.MatchedPaymentId)
            .HasColumnName("matched_payment_id");

        builder.Property(i => i.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.DiscrepancyReason)
            .HasColumnName("discrepancy_reason")
            .HasMaxLength(500);

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(i => i.ReconciliationId)
            .HasDatabaseName("ix_reconciliation_items_reconciliation_id");

        builder.HasIndex(i => i.Status)
            .HasDatabaseName("ix_reconciliation_items_status");
    }
}
