using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.NumberInventory.Domain.Entities;

namespace Obss.NumberInventory.Infrastructure.Persistence.Configurations;

public sealed class TelephoneNumberConfiguration : IEntityTypeConfiguration<TelephoneNumber>
{
    public void Configure(EntityTypeBuilder<TelephoneNumber> builder)
    {
        builder.ToTable("telephone_numbers");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .ValueGeneratedNever();

        builder.Property(n => n.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(n => n.Number)
            .HasColumnName("number")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(n => n.NumberType)
            .HasColumnName("number_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(n => n.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(n => n.CustomerId)
            .HasColumnName("customer_id");

        builder.Property(n => n.SubscriptionId)
            .HasColumnName("subscription_id");

        builder.Property(n => n.AssignedAt)
            .HasColumnName("assigned_at");

        builder.Property(n => n.ReservedAt)
            .HasColumnName("reserved_at");

        builder.Property(n => n.Cost)
            .HasColumnName("cost")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(n => n.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(n => n.Notes)
            .HasColumnName("notes")
            .HasMaxLength(500);

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(n => n.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(n => n.Number)
            .IsUnique()
            .HasDatabaseName("ix_telephone_numbers_number");

        builder.HasIndex(n => n.CustomerId)
            .HasDatabaseName("ix_telephone_numbers_customer_id");

        builder.HasIndex(n => n.Status)
            .HasDatabaseName("ix_telephone_numbers_status");

        builder.HasIndex(n => n.TenantId)
            .HasDatabaseName("ix_telephone_numbers_tenant_id");

        builder.HasIndex(n => n.NumberType)
            .HasDatabaseName("ix_telephone_numbers_number_type");
    }
}
