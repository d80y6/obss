using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.CRM.Domain.Entities;
using Obss.SharedKernel.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.CRM.Infrastructure.Persistence.Configurations;

public sealed class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("contacts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(c => c.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasConversion<EmailValueConverter>()
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(c => c.PhoneNumber)
            .HasColumnName("phone_number")
            .HasConversion<PhoneNumberValueConverter>()
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(c => c.MobileNumber)
            .HasColumnName("mobile_number")
            .HasConversion<PhoneNumberValueConverter>()
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(c => c.Position)
            .HasColumnName("position")
            .HasMaxLength(100);

        builder.Property(c => c.IsPrimary)
            .HasColumnName("is_primary")
            .IsRequired();

        builder.Property(c => c.IsBilling)
            .HasColumnName("is_billing")
            .IsRequired();

        builder.Property(c => c.IsTechnical)
            .HasColumnName("is_technical")
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(c => c.CustomerId)
            .HasDatabaseName("ix_contacts_customer_id");

        builder.HasIndex(c => c.Email)
            .HasDatabaseName("ix_contacts_email");

        builder.Ignore(c => c.Customer);
    }
}
