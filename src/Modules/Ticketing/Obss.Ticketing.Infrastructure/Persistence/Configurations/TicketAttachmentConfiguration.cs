using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Ticketing.Domain.Entities;

namespace Obss.Ticketing.Infrastructure.Persistence.Configurations;

public sealed class TicketAttachmentConfiguration : IEntityTypeConfiguration<TicketAttachment>
{
    public void Configure(EntityTypeBuilder<TicketAttachment> builder)
    {
        builder.ToTable("ticket_attachments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.TicketId)
            .HasColumnName("ticket_id")
            .IsRequired();

        builder.Property(a => a.FileName)
            .HasColumnName("file_name")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(a => a.ContentType)
            .HasColumnName("content_type")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.FileSize)
            .HasColumnName("file_size")
            .IsRequired();

        builder.Property(a => a.StoragePath)
            .HasColumnName("storage_path")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(a => a.UploadedById)
            .HasColumnName("uploaded_by_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(a => a.TicketId)
            .HasDatabaseName("ix_ticket_attachments_ticket_id");
    }
}
