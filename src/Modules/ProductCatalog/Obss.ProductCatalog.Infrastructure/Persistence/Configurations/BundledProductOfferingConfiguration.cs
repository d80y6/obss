using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ProductCatalog.Domain.Domain.Entities;

namespace Obss.ProductCatalog.Infrastructure.Persistence.Configurations;

public sealed class BundledProductOfferingConfiguration : IEntityTypeConfiguration<BundledProductOffering>
{
    public void Configure(EntityTypeBuilder<BundledProductOffering> builder)
    {
        builder.ToTable("bundled_product_offerings");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).ValueGeneratedNever();

        builder.Property(b => b.OfferId).HasColumnName("offer_id").IsRequired();
        builder.Property(b => b.BundledOfferId).HasColumnName("bundled_offer_id").IsRequired();
        builder.Property(b => b.Name).HasColumnName("name").HasMaxLength(200);
        builder.Property(b => b.Quantity).HasColumnName("quantity").IsRequired();
        builder.Property(b => b.ReferralType).HasColumnName("referral_type").HasMaxLength(50);

        builder.HasOne(b => b.BundledOffer)
            .WithMany()
            .HasForeignKey(b => b.BundledOfferId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(b => b.OfferId).HasDatabaseName("ix_bundled_product_offerings_offer_id");
        builder.HasIndex(b => b.BundledOfferId).HasDatabaseName("ix_bundled_product_offerings_bundled_offer_id");
        builder.HasIndex(b => new { b.OfferId, b.BundledOfferId })
            .HasDatabaseName("ix_bundled_product_offerings_offer_bundled")
            .IsUnique();
    }
}
