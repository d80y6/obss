using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;

namespace Obss.ProductCatalog.Infrastructure.Persistence.Configurations;

public sealed class ProductConfigurationRuleConfiguration : IEntityTypeConfiguration<ProductConfigurationRule>
{
    public void Configure(EntityTypeBuilder<ProductConfigurationRule> builder)
    {
        builder.ToTable("product_configuration_rules");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(r => r.RuleType)
            .HasColumnName("rule_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.TargetProductId)
            .HasColumnName("target_product_id");

        builder.Property(r => r.TargetOption)
            .HasColumnName("target_option")
            .HasMaxLength(200);

        builder.Property(r => r.Condition)
            .HasColumnName("condition")
            .HasMaxLength(4000);

        builder.Property(r => r.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.HasIndex(r => r.ProductId).HasDatabaseName("ix_product_configuration_rules_product_id");
        builder.HasIndex(r => r.RuleType).HasDatabaseName("ix_product_configuration_rules_rule_type");
    }
}
