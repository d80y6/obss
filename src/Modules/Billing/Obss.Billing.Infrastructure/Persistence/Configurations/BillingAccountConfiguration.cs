using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Billing.Domain.Entities;

namespace Obss.Billing.Infrastructure.Persistence.Configurations;

public sealed class BillingAccountConfiguration : IEntityTypeConfiguration<BillingAccount>
{
    public void Configure(EntityTypeBuilder<BillingAccount> builder)
    {
        builder.ToTable("billing_accounts");

        builder.HasKey(ba => ba.Id);

        builder.Property(ba => ba.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(ba => ba.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ba => ba.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(ba => ba.AccountType)
            .HasColumnName("account_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(ba => ba.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(ba => ba.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(ba => ba.CreditLimit)
            .HasColumnName("credit_limit")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(ba => ba.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(ba => ba.ValidFrom)
            .HasColumnName("valid_from");

        builder.Property(ba => ba.ValidUntil)
            .HasColumnName("valid_until");

        builder.Property(ba => ba.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(ba => ba.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(ba => ba.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(ba => ba.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(ba => ba.Href)
            .HasColumnName("href")
            .HasMaxLength(500);

        builder.Property(ba => ba.AtType)
            .HasColumnName("at_type")
            .HasMaxLength(100);

        builder.Property(ba => ba.AtBaseType)
            .HasColumnName("at_base_type")
            .HasMaxLength(100);

        builder.Property(ba => ba.AtSchemaLocation)
            .HasColumnName("at_schema_location")
            .HasMaxLength(500);

        builder.Property(ba => ba.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(100);

        builder.Property(ba => ba.PaymentMethodId)
            .HasColumnName("payment_method_id")
            .HasMaxLength(100);

        builder.OwnsOne(ba => ba.AccountHolder, ah =>
        {
            ah.Property(a => a.Name).HasColumnName("account_holder_name").HasMaxLength(200);
            ah.Property(a => a.Email).HasColumnName("account_holder_email").HasMaxLength(200);
            ah.Property(a => a.Phone).HasColumnName("account_holder_phone").HasMaxLength(50);
            ah.Property(a => a.ContactId).HasColumnName("account_holder_contact_id");
        });

        builder.OwnsMany(ba => ba.BillPresentationMedia, pm =>
        {
            pm.ToTable("billing_account_presentation_media");
            pm.WithOwner().HasForeignKey("billing_account_id");
            pm.HasKey("Id");

            pm.Property(p => p.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            pm.Property(p => p.MediaType)
                .HasColumnName("media_type")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            pm.Property(p => p.EmailAddress)
                .HasColumnName("email_address")
                .HasMaxLength(200);

            pm.Property(p => p.PaperFormat)
                .HasColumnName("paper_format")
                .HasMaxLength(10);

            pm.Property(p => p.Language)
                .HasColumnName("language")
                .HasMaxLength(10)
                .IsRequired();

            pm.Property(p => p.IsPreferred)
                .HasColumnName("is_preferred")
                .IsRequired();

            pm.Property(p => p.ValidFrom)
                .HasColumnName("valid_from");

            pm.Property(p => p.ValidUntil)
                .HasColumnName("valid_until");
        });

        builder.Navigation(ba => ba.BillPresentationMedia)
            .AutoInclude();

        builder.OwnsMany(ba => ba.RelatedParties, rp =>
        {
            rp.ToTable("billing_account_related_parties");
            rp.WithOwner().HasForeignKey("billing_account_id");
            rp.Property(r => r.PartyId).HasColumnName("party_id").HasMaxLength(100);
            rp.Property(r => r.PartyName).HasColumnName("party_name").HasMaxLength(200);
            rp.Property(r => r.Role).HasColumnName("role").HasMaxLength(50);
        });

        builder.Navigation(ba => ba.RelatedParties)
            .AutoInclude();

        builder.HasIndex(ba => ba.CustomerId)
            .HasDatabaseName("ix_billing_accounts_customer_id");
    }
}
