using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Payments.Domain.Entities;

namespace Obss.Payments.Infrastructure.Persistence.Configurations;

public sealed class PaymentReconciliationConfiguration : IEntityTypeConfiguration<PaymentReconciliation>
{
    public void Configure(EntityTypeBuilder<PaymentReconciliation> builder)
    {
        builder.ToTable("payment_reconciliations");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.ImportDate)
            .HasColumnName("import_date")
            .IsRequired();

        builder.Property(r => r.ImportSource)
            .HasColumnName("import_source")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.ImportFileName)
            .HasColumnName("import_file_name")
            .HasMaxLength(500);

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.TotalImportAmount)
            .HasColumnName("total_import_amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(r => r.TotalReconciledAmount)
            .HasColumnName("total_reconciled_amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(r => r.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(r => r.ImportedBy)
            .HasColumnName("imported_by")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasMany(r => r.Items)
            .WithOne()
            .HasForeignKey(i => i.ReconciliationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.TenantId)
            .HasDatabaseName("ix_payment_reconciliations_tenant_id");

        builder.HasIndex(r => r.Status)
            .HasDatabaseName("ix_payment_reconciliations_status");

        builder.Navigation(r => r.Items)
            .AutoInclude();
    }
}
