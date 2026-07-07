using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Rating.Domain.Entities;

namespace Obss.Rating.Infrastructure.Persistence.Configurations;

public sealed class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.ToTable("promotions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(p => p.PromotionType)
            .HasColumnName("promotion_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.DiscountValue)
            .HasColumnName("discount_value")
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(p => p.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(p => p.MinQuantity)
            .HasColumnName("min_quantity");

        builder.Property(p => p.MaxQuantity)
            .HasColumnName("max_quantity");

        builder.Property(p => p.ValidFrom)
            .HasColumnName("valid_from")
            .IsRequired();

        builder.Property(p => p.ValidTo)
            .HasColumnName("valid_to");

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(p => p.IsStackable)
            .HasColumnName("is_stackable")
            .IsRequired();

        builder.Property(p => p.Priority)
            .HasColumnName("priority")
            .IsRequired();

        builder.Property(p => p.Code)
            .HasColumnName("code")
            .HasMaxLength(50);

        builder.Property(p => p.MaxRedemptions)
            .HasColumnName("max_redemptions");

        builder.Property(p => p.CurrentRedemptions)
            .HasColumnName("current_redemptions")
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(p => p.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(100);

        builder.HasMany(p => p.Rules)
            .WithOne()
            .HasForeignKey(r => r.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.TenantId)
            .HasDatabaseName("ix_promotions_tenant_id");

        builder.HasIndex(p => p.Code)
            .HasDatabaseName("ix_promotions_code")
            .IsUnique()
            .HasFilter("code IS NOT NULL");

        builder.HasIndex(p => new { p.IsActive, p.ValidFrom, p.ValidTo })
            .HasDatabaseName("ix_promotions_is_active_valid_range");
    }
}
