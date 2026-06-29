using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.CRM.Domain.Entities;

namespace Obss.CRM.Infrastructure.Persistence.Configurations;

public sealed class CustomerNoteConfiguration : IEntityTypeConfiguration<CustomerNote>
{
    public void Configure(EntityTypeBuilder<CustomerNote> builder)
    {
        builder.ToTable("customer_notes");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .ValueGeneratedNever();

        builder.Property(n => n.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(n => n.Content)
            .HasColumnName("content")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(n => n.Category)
            .HasColumnName("category")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(n => n.CreatedById)
            .HasColumnName("created_by_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(n => n.CustomerId)
            .HasDatabaseName("ix_customer_notes_customer_id");

        builder.HasIndex(n => n.Category)
            .HasDatabaseName("ix_customer_notes_category");
    }
}
