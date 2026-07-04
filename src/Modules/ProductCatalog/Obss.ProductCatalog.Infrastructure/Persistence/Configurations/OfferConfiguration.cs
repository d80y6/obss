using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ProductCatalog.Domain.Domain.Entities;

namespace Obss.ProductCatalog.Infrastructure.Persistence.Configurations;

public sealed class OfferConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> builder)
    {
        builder.ToTable("offers");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .ValueGeneratedNever();

        builder.Property(o => o.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(o => o.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(o => o.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        builder.Property(o => o.OfferType)
            .HasColumnName("offer_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(o => o.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(o => o.IsContract)
            .HasColumnName("is_contract")
            .IsRequired();

        builder.Property(o => o.ContractDurationMonths)
            .HasColumnName("contract_duration_months");

        builder.Property(o => o.BillingPeriod)
            .HasColumnName("billing_period")
            .HasConversion<string?>()
            .HasMaxLength(50);

        builder.Property(o => o.TaxInclusive)
            .HasColumnName("tax_inclusive")
            .IsRequired();

        builder.Property(o => o.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired();

        builder.Property(o => o.ValidFrom)
            .HasColumnName("valid_from");

        builder.Property(o => o.ValidTo)
            .HasColumnName("valid_to");

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasMany(o => o.OfferPricings)
            .WithOne()
            .HasForeignKey(op => op.OfferId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Discounts)
            .WithOne()
            .HasForeignKey(od => od.OfferId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Terms)
            .WithOne()
            .HasForeignKey(t => t.OfferId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => o.TenantId).HasDatabaseName("ix_offers_tenant_id");
        builder.HasIndex(o => o.OfferType).HasDatabaseName("ix_offers_offer_type");
        builder.HasIndex(o => o.IsActive).HasDatabaseName("ix_offers_is_active");
        builder.HasIndex(o => new { o.TenantId, o.Name }).HasDatabaseName("ix_offers_tenant_id_name");
    }
}
