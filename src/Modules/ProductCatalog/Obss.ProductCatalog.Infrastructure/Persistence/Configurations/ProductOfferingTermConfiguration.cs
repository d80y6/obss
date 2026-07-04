using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ProductCatalog.Domain.Domain.Entities;

namespace Obss.ProductCatalog.Infrastructure.Persistence.Configurations;

public sealed class ProductOfferingTermConfiguration : IEntityTypeConfiguration<ProductOfferingTerm>
{
    public void Configure(EntityTypeBuilder<ProductOfferingTerm> builder)
    {
        builder.ToTable("product_offering_terms");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.OfferId).HasColumnName("offer_id").IsRequired();
        builder.Property(t => t.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(t => t.Description).HasColumnName("description").HasMaxLength(2000);
        builder.Property(t => t.Duration).HasColumnName("duration").IsRequired();

        builder.Property(t => t.DurationUnit)
            .HasColumnName("duration_unit")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.TermType)
            .HasColumnName("term_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.ValidFrom).HasColumnName("valid_from");
        builder.Property(t => t.ValidTo).HasColumnName("valid_to");

        builder.HasIndex(t => t.OfferId).HasDatabaseName("ix_product_offering_terms_offer_id");
        builder.HasIndex(t => t.TermType).HasDatabaseName("ix_product_offering_terms_term_type");
    }
}
