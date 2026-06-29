using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.CRM.Domain.Entities;
using Obss.SharedKernel.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.CRM.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.CustomerType)
            .HasColumnName("customer_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.CompanyName)
            .HasColumnName("company_name")
            .HasMaxLength(200);

        builder.Property(c => c.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.TaxNumber)
            .HasColumnName("tax_number")
            .HasMaxLength(50);

        builder.Property(c => c.RegistrationNumber)
            .HasColumnName("registration_number")
            .HasMaxLength(50);

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

        builder.Property(c => c.Website)
            .HasColumnName("website")
            .HasMaxLength(500);

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(c => c.CreditLimit)
            .HasColumnName("credit_limit")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(c => c.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasMany(c => c.Contacts)
            .WithOne(co => co.Customer)
            .HasForeignKey(co => co.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Notes)
            .WithOne(n => n.Customer)
            .HasForeignKey(n => n.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.TenantId)
            .HasDatabaseName("ix_customers_tenant_id");

        builder.HasIndex(c => c.Status)
            .HasDatabaseName("ix_customers_status");

        builder.HasIndex(c => c.CustomerType)
            .HasDatabaseName("ix_customers_customer_type");

        builder.HasIndex(c => c.DisplayName)
            .HasDatabaseName("ix_customers_display_name");

        builder.HasIndex(c => c.Email)
            .HasDatabaseName("ix_customers_email");

        builder.Navigation(c => c.Contacts)
            .AutoInclude();

        builder.Navigation(c => c.Notes)
            .AutoInclude();
    }
}
