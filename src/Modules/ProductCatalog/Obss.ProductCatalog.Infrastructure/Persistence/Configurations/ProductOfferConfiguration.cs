using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ProductCatalog.Domain.Domain.Entities;

namespace Obss.ProductCatalog.Infrastructure.Persistence.Configurations;

public sealed class ProductOfferConfiguration : IEntityTypeConfiguration<ProductOffer>
{
    public void Configure(EntityTypeBuilder<ProductOffer> builder)
    {
        builder.ToTable("product_offers");

        builder.HasKey(po => po.Id);

        builder.Property(po => po.Id)
            .ValueGeneratedNever();

        builder.Property(po => po.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(po => po.OfferId)
            .HasColumnName("offer_id")
            .IsRequired();

        builder.Property(po => po.IsPrimary)
            .HasColumnName("is_primary")
            .IsRequired();

        builder.Property(po => po.IsRequired)
            .HasColumnName("is_required")
            .IsRequired();

        builder.Property(po => po.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(po => po.ProductId).HasDatabaseName("ix_product_offers_product_id");
        builder.HasIndex(po => po.OfferId).HasDatabaseName("ix_product_offers_offer_id");
        builder.HasIndex(po => new { po.ProductId, po.OfferId }).HasDatabaseName("ix_product_offers_product_offer").IsUnique();
    }
}
