using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.CRM.Domain.Entities;

namespace Obss.CRM.Infrastructure.Persistence.Configurations;

public sealed class QuoteItemConfiguration : IEntityTypeConfiguration<QuoteItem>
{
    public void Configure(EntityTypeBuilder<QuoteItem> builder)
    {
        builder.ToTable("quote_items");

        builder.HasKey(qi => qi.Id);
        builder.Property(qi => qi.Id).ValueGeneratedNever();

        builder.Property(qi => qi.Action).HasColumnName("action").IsRequired().HasMaxLength(50).HasConversion<string>();
        builder.Property(qi => qi.State).HasColumnName("state").IsRequired().HasMaxLength(50).HasConversion<string>();
        builder.Property(qi => qi.Quantity).HasColumnName("quantity").IsRequired();
        builder.Property(qi => qi.ProductOfferingId).HasColumnName("product_offering_id");
        builder.Property(qi => qi.ProductOfferingName).HasColumnName("product_offering_name").HasMaxLength(200);
        builder.Property(qi => qi.ProductId).HasColumnName("product_id");
        builder.Property(qi => qi.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(qi => qi.UpdatedAt).HasColumnName("updated_at").IsRequired();

        // JSON columns for value object collections on QuoteItem
        builder.OwnsMany(qi => qi.Prices, prices =>
        {
            prices.ToJson("prices");
        });
        builder.OwnsMany(qi => qi.ItemRelationships, rel =>
        {
            rel.ToJson("item_relationships");
        });
        builder.OwnsMany(qi => qi.Notes, notes =>
        {
            notes.ToJson("notes");
        });
    }
}
