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

        builder.Property(c => c.IndividualId)
            .HasColumnName("individual_id");

        builder.Property(c => c.OrganizationId)
            .HasColumnName("organization_id");

        builder.Property(c => c.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        builder.Property(c => c.StatusReason)
            .HasColumnName("status_reason")
            .HasMaxLength(500);

        builder.Property(c => c.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(100);

        builder.Property(c => c.Href)
            .HasColumnName("href")
            .HasMaxLength(500);

        builder.OwnsOne(c => c.ValidFor, vf =>
        {
            vf.Property(p => p.StartDateTime).HasColumnName("valid_from");
            vf.Property(p => p.EndDateTime).HasColumnName("valid_until");
        });

        builder.HasOne(c => c.Individual)
            .WithMany()
            .HasForeignKey(c => c.IndividualId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Organization)
            .WithMany()
            .HasForeignKey(c => c.OrganizationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.OwnsMany(c => c.Characteristics, ch =>
        {
            ch.ToTable("customer_characteristics");
            ch.Property<int>("Id").ValueGeneratedOnAdd();
            ch.HasKey("Id");
            ch.Property(c => c.Key).HasColumnName("key").HasMaxLength(100).IsRequired();
            ch.Property(c => c.Value).HasColumnName("value").HasMaxLength(500);
            ch.Property(c => c.ValueType).HasColumnName("value_type").HasMaxLength(30);
            ch.WithOwner().HasForeignKey("customer_id");
        });

        builder.HasMany(c => c.CreditProfiles)
            .WithOne()
            .HasForeignKey(cp => cp.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsMany(c => c.RelatedParties, rp =>
        {
            rp.ToTable("customer_related_parties");
            rp.Property<int>("Id").ValueGeneratedOnAdd();
            rp.HasKey("Id");
            rp.Property(r => r.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            rp.Property(r => r.Role).HasColumnName("role").HasMaxLength(50).IsRequired();
            rp.Property(r => r.ReferredId).HasColumnName("referred_id").IsRequired();
            rp.Property(r => r.ReferredType).HasColumnName("referred_type").HasMaxLength(50).IsRequired();
            rp.WithOwner().HasForeignKey("customer_id");
        });

        builder.OwnsMany(c => c.NotificationHubs, nh =>
        {
            nh.ToTable("customer_notification_hubs");
            nh.Property<int>("Id").ValueGeneratedOnAdd();
            nh.HasKey("Id");
            nh.Property(n => n.HubType).HasMaxLength(20).HasConversion<string>();
            nh.Property(n => n.Identifier).HasMaxLength(200).IsRequired();
            nh.Property(n => n.IsOptIn).IsRequired();
            nh.Property(n => n.ValidFrom);
            nh.Property(n => n.ValidUntil);
            nh.WithOwner().HasForeignKey("customer_id");
        });

        builder.OwnsMany(c => c.ContactMedia, cm =>
        {
            cm.ToTable("customer_contact_media");
            cm.Property<int>("Id").ValueGeneratedOnAdd();
            cm.HasKey("Id");
            cm.Property(m => m.MediumType).HasMaxLength(20).HasConversion<string>();
            cm.Property(m => m.IsPreferred).IsRequired();
            cm.Property(m => m.ValidFrom);
            cm.Property(m => m.ValidUntil);
            cm.WithOwner().HasForeignKey("customer_id");

            cm.OwnsMany(m => m.Characteristics, cc =>
            {
                cc.ToTable("customer_contact_medium_characteristics");
                cc.Property<int>("Id").ValueGeneratedOnAdd();
                cc.HasKey("Id");
                cc.Property(c => c.Key).HasMaxLength(100).IsRequired();
                cc.Property(c => c.Value).HasMaxLength(500).IsRequired();
                cc.Property(c => c.ValueType).HasMaxLength(30).IsRequired();
                cc.WithOwner().HasForeignKey("contact_medium_id");
            });
        });

        builder.OwnsMany(c => c.AccountRefs, ar =>
        {
            ar.ToTable("customer_account_refs");
            ar.Property<int>("Id").ValueGeneratedOnAdd();
            ar.HasKey("Id");
            ar.Property(r => r.BillingAccountId).IsRequired();
            ar.Property(r => r.Name).HasMaxLength(200).IsRequired();
            ar.Property(r => r.AccountType).HasMaxLength(30).IsRequired();
            ar.Property(r => r.Role).HasMaxLength(20).IsRequired();
            ar.Property(r => r.Href).HasMaxLength(500);
            ar.WithOwner().HasForeignKey("customer_id");
        });

        builder.OwnsMany(c => c.AgreementRefs, ar =>
        {
            ar.ToTable("customer_agreement_refs");
            ar.Property<int>("Id").ValueGeneratedOnAdd();
            ar.HasKey("Id");
            ar.Property(r => r.AgreementId).IsRequired();
            ar.Property(r => r.Name).HasMaxLength(200).IsRequired();
            ar.Property(r => r.AgreementType).HasMaxLength(30).IsRequired();
            ar.Property(r => r.Role).HasMaxLength(20).IsRequired();
            ar.Property(r => r.Href).HasMaxLength(500);
            ar.WithOwner().HasForeignKey("customer_id");
        });

        builder.OwnsMany(c => c.PaymentMethodRefs, pmr =>
        {
            pmr.ToTable("customer_payment_method_refs");
            pmr.Property<int>("Id").ValueGeneratedOnAdd();
            pmr.HasKey("Id");
            pmr.Property(r => r.PaymentMethodId).IsRequired();
            pmr.Property(r => r.Name).HasMaxLength(200).IsRequired();
            pmr.Property(r => r.Href).HasMaxLength(500);
            pmr.WithOwner().HasForeignKey("customer_id");
        });

        builder.Navigation(c => c.Characteristics)
            .AutoInclude();

        builder.Navigation(c => c.CreditProfiles)
            .AutoInclude();

        builder.Navigation(c => c.RelatedParties)
            .AutoInclude();

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
