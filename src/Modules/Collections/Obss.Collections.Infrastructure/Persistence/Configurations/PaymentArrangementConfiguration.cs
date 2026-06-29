using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Collections.Domain.Entities;

namespace Obss.Collections.Infrastructure.Persistence.Configurations;

public sealed class PaymentArrangementConfiguration : IEntityTypeConfiguration<PaymentArrangement>
{
    public void Configure(EntityTypeBuilder<PaymentArrangement> builder)
    {
        builder.ToTable("payment_arrangements");

        builder.HasKey(pa => pa.Id);

        builder.Property(pa => pa.Id)
            .ValueGeneratedNever();

        builder.Property(pa => pa.CollectionCaseId)
            .HasColumnName("collection_case_id")
            .IsRequired();

        builder.Property(pa => pa.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(pa => pa.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(pa => pa.TotalAmount)
            .HasColumnName("total_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(pa => pa.PaidAmount)
            .HasColumnName("paid_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(pa => pa.InstallmentCount)
            .HasColumnName("installment_count")
            .IsRequired();

        builder.Property(pa => pa.InstallmentAmount)
            .HasColumnName("installment_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(pa => pa.Frequency)
            .HasColumnName("frequency")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(pa => pa.FirstPaymentDate)
            .HasColumnName("first_payment_date")
            .IsRequired();

        builder.Property(pa => pa.LastPaymentDate)
            .HasColumnName("last_payment_date");

        builder.Property(pa => pa.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(pa => pa.DefaultedAt)
            .HasColumnName("defaulted_at");

        builder.HasMany(pa => pa.Installments)
            .WithOne()
            .HasForeignKey(i => i.PaymentArrangementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(pa => pa.Installments)
            .AutoInclude();

        builder.HasIndex(pa => pa.CollectionCaseId)
            .HasDatabaseName("ix_payment_arrangements_case_id");

        builder.HasIndex(pa => pa.Status)
            .HasDatabaseName("ix_payment_arrangements_status");
    }
}
