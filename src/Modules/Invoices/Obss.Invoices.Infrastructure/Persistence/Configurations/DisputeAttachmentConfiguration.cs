using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Invoices.Domain.Entities;

namespace Obss.Invoices.Infrastructure.Persistence.Configurations;

public sealed class DisputeAttachmentConfiguration : IEntityTypeConfiguration<DisputeAttachment>
{
    public void Configure(EntityTypeBuilder<DisputeAttachment> builder)
    {
        builder.ToTable("dispute_attachments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.InvoiceDisputeId)
            .HasColumnName("invoice_dispute_id")
            .IsRequired();

        builder.Property(a => a.FileName)
            .HasColumnName("file_name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.ContentType)
            .HasColumnName("content_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.FileSize)
            .HasColumnName("file_size")
            .IsRequired();

        builder.Property(a => a.StoragePath)
            .HasColumnName("storage_path")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(a => a.UploadedAt)
            .HasColumnName("uploaded_at")
            .IsRequired();

        builder.HasIndex(a => a.InvoiceDisputeId)
            .HasDatabaseName("ix_dispute_attachments_invoice_dispute_id");
    }
}
