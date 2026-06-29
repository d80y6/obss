using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Rating.Domain.Entities;

namespace Obss.Rating.Infrastructure.Persistence.Configurations;

public sealed class RatingRuleConfiguration : IEntityTypeConfiguration<RatingRule>
{
    public void Configure(EntityTypeBuilder<RatingRule> builder)
    {
        builder.ToTable("rating_rules");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(r => r.RuleType)
            .HasColumnName("rule_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.ProductId)
            .HasColumnName("product_id");

        builder.Property(r => r.OfferId)
            .HasColumnName("offer_id");

        builder.Property(r => r.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(r => r.Priority)
            .HasColumnName("priority")
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.OwnsMany(r => r.Tiers, tb =>
        {
            tb.ToTable("rating_rule_tiers");
            tb.WithOwner().HasForeignKey("rating_rule_id");
            tb.HasKey("Id");

            tb.Property(t => t.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            tb.Property(t => t.FromUnit)
                .HasColumnName("from_unit")
                .IsRequired();

            tb.Property(t => t.ToUnit)
                .HasColumnName("to_unit");

            tb.Property(t => t.Rate)
                .HasColumnName("rate")
                .HasPrecision(18, 6)
                .IsRequired();

            tb.Property(t => t.Description)
                .HasColumnName("description")
                .HasMaxLength(500);
        });

        builder.HasIndex(r => r.TenantId)
            .HasDatabaseName("ix_rating_rules_tenant_id");

        builder.HasIndex(r => new { r.TenantId, r.Name })
            .HasDatabaseName("ix_rating_rules_tenant_id_name")
            .IsUnique();

        builder.HasIndex(r => new { r.IsActive, r.Priority })
            .HasDatabaseName("ix_rating_rules_is_active_priority");
    }
}
