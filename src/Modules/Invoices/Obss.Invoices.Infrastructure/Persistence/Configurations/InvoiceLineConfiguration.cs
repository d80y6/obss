using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Invoices.Domain.Entities;

namespace Obss.Invoices.Infrastructure.Persistence.Configurations;

public sealed class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("invoice_lines");

        builder.HasKey(il => il.Id);

        builder.Property(il => il.Id)
            .ValueGeneratedNever();

        builder.Property(il => il.InvoiceId)
            .HasColumnName("invoice_id")
            .IsRequired();

        builder.Property(il => il.BillId)
            .HasColumnName("bill_id")
            .IsRequired();

        builder.Property(il => il.BillLineId)
            .HasColumnName("bill_line_id");

        builder.Property(il => il.LineType)
            .HasColumnName("line_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(il => il.Description)
            .HasColumnName("description")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(il => il.Quantity)
            .HasColumnName("quantity")
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(il => il.UnitPrice)
            .HasColumnName("unit_price")
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(il => il.TotalAmount)
            .HasColumnName("total_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(il => il.TaxAmount)
            .HasColumnName("tax_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(il => il.TaxRate)
            .HasColumnName("tax_rate")
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(il => il.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(il => il.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(100);

        builder.HasIndex(il => il.InvoiceId)
            .HasDatabaseName("ix_invoice_lines_invoice_id");

        builder.HasIndex(il => il.BillId)
            .HasDatabaseName("ix_invoice_lines_bill_id");
    }
}
