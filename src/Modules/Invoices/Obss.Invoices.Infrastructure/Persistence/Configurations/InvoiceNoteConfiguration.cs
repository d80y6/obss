using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Invoices.Domain.Entities;

namespace Obss.Invoices.Infrastructure.Persistence.Configurations;

public sealed class InvoiceNoteConfiguration : IEntityTypeConfiguration<InvoiceNote>
{
    public void Configure(EntityTypeBuilder<InvoiceNote> builder)
    {
        builder.ToTable("invoice_notes");

        builder.HasKey(inn => inn.Id);

        builder.Property(inn => inn.Id)
            .ValueGeneratedNever();

        builder.Property(inn => inn.InvoiceId)
            .HasColumnName("invoice_id")
            .IsRequired();

        builder.Property(inn => inn.Content)
            .HasColumnName("content")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(inn => inn.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(inn => inn.InvoiceId)
            .HasDatabaseName("ix_invoice_notes_invoice_id");
    }
}
