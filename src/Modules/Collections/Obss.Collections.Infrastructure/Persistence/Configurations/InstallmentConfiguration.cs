using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Collections.Domain.Entities;

namespace Obss.Collections.Infrastructure.Persistence.Configurations;

public sealed class InstallmentConfiguration : IEntityTypeConfiguration<Installment>
{
    public void Configure(EntityTypeBuilder<Installment> builder)
    {
        builder.ToTable("installments");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(i => i.PaymentArrangementId)
            .HasColumnName("payment_arrangement_id")
            .IsRequired();

        builder.Property(i => i.InstallmentNumber)
            .HasColumnName("installment_number")
            .IsRequired();

        builder.Property(i => i.DueDate)
            .HasColumnName("due_date")
            .IsRequired();

        builder.Property(i => i.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.PaidAmount)
            .HasColumnName("paid_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(i => i.PaidAt)
            .HasColumnName("paid_at");

        builder.HasIndex(i => i.PaymentArrangementId)
            .HasDatabaseName("ix_installments_arrangement_id");
    }
}
