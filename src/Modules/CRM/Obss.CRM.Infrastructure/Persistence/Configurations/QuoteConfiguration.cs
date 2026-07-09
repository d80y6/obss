using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.CRM.Domain.Entities;
using Obss.CRM.Domain.ValueObjects;

namespace Obss.CRM.Infrastructure.Persistence.Configurations;

public sealed class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.ToTable("quotes");

        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id).ValueGeneratedNever();

        builder.Property(q => q.TenantId).HasColumnName("tenant_id").IsRequired().HasMaxLength(100);
        builder.Property(q => q.ExternalId).HasColumnName("external_id").HasMaxLength(100);
        builder.Property(q => q.State).HasColumnName("state").IsRequired().HasMaxLength(50).HasConversion<string>();
        builder.Property(q => q.QuoteDate).HasColumnName("quote_date").IsRequired();
        builder.Property(q => q.Category).HasColumnName("category").HasMaxLength(100);
        builder.Property(q => q.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(q => q.Version).HasColumnName("version").IsRequired();
        builder.Property(q => q.ValidFrom).HasColumnName("valid_from");
        builder.Property(q => q.ValidUntil).HasColumnName("valid_until");
        builder.Property(q => q.ExpectedQuoteCompletionDate).HasColumnName("expected_quote_completion_date");
        builder.Property(q => q.EffectiveQuoteCompletionDate).HasColumnName("effective_quote_completion_date");
        builder.Property(q => q.ExpectedFulfillmentStartDate).HasColumnName("expected_fulfillment_start_date");
        builder.Property(q => q.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(q => q.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(q => q.UpdatedAt).HasColumnName("updated_at").IsRequired();

        // JSON columns for value object collections
        builder.OwnsMany(q => q.RelatedParties, related =>
        {
            related.ToJson("related_parties");
            related.OwnedEntityType.AddProperty("Id", typeof(Guid));
        });
        builder.OwnsMany(q => q.QuotePrices, prices =>
        {
            prices.ToJson("quote_prices");
        });
        builder.OwnsMany(q => q.Authorizations, auth =>
        {
            auth.ToJson("quote_authorizations");
        });
        builder.OwnsMany(q => q.BillingAccountRefs, billing =>
        {
            billing.ToJson("billing_account_refs");
            billing.OwnedEntityType.AddProperty("Id", typeof(Guid));
        });
        builder.OwnsMany(q => q.AgreementRefs, agreements =>
        {
            agreements.ToJson("agreement_refs");
            agreements.OwnedEntityType.AddProperty("Id", typeof(Guid));
        });
        builder.OwnsMany(q => q.Notes, notes =>
        {
            notes.ToJson("notes");
        });

        // QuoteItem is a separate table with FK
        builder.HasMany(q => q.Items)
            .WithOne()
            .HasForeignKey("QuoteId")
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(q => q.TenantId).HasDatabaseName("ix_quotes_tenant_id");
        builder.HasIndex(q => q.CustomerId).HasDatabaseName("ix_quotes_customer_id");
        builder.HasIndex(q => q.State).HasDatabaseName("ix_quotes_state");
    }
}
