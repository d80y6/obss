using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Invoices.Domain.Entities;

namespace Obss.Invoices.Infrastructure.Persistence.Configurations;

public sealed class CreditNoteLineConfiguration : IEntityTypeConfiguration<CreditNoteLine>
{
    public void Configure(EntityTypeBuilder<CreditNoteLine> builder)
    {
        builder.ToTable("credit_note_lines");

        builder.HasKey(cl => cl.Id);

        builder.Property(cl => cl.Id)
            .ValueGeneratedNever();

        builder.Property(cl => cl.CreditNoteId)
            .HasColumnName("credit_note_id")
            .IsRequired();

        builder.Property(cl => cl.InvoiceLineId)
            .HasColumnName("invoice_line_id")
            .IsRequired();

        builder.Property(cl => cl.Description)
            .HasColumnName("description")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(cl => cl.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(cl => cl.Quantity)
            .HasColumnName("quantity")
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(cl => cl.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(100);

        builder.HasIndex(cl => cl.CreditNoteId)
            .HasDatabaseName("ix_credit_note_lines_credit_note_id");

        builder.HasIndex(cl => cl.InvoiceLineId)
            .HasDatabaseName("ix_credit_note_lines_invoice_line_id");
    }
}
