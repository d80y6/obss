using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Ticketing.Domain.Entities;

namespace Obss.Ticketing.Infrastructure.Persistence.Configurations;

public sealed class TicketCommentConfiguration : IEntityTypeConfiguration<TicketComment>
{
    public void Configure(EntityTypeBuilder<TicketComment> builder)
    {
        builder.ToTable("ticket_comments");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.TicketId)
            .HasColumnName("ticket_id")
            .IsRequired();

        builder.Property(c => c.Content)
            .HasColumnName("content")
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(c => c.IsInternal)
            .HasColumnName("is_internal")
            .IsRequired();

        builder.Property(c => c.AuthorId)
            .HasColumnName("author_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.AuthorName)
            .HasColumnName("author_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(c => c.TicketId)
            .HasDatabaseName("ix_ticket_comments_ticket_id");

        builder.HasIndex(c => c.AuthorId)
            .HasDatabaseName("ix_ticket_comments_author_id");
    }
}
