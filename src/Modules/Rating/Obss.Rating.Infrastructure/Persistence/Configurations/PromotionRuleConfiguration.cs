using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Rating.Domain.Entities;

namespace Obss.Rating.Infrastructure.Persistence.Configurations;

public sealed class PromotionRuleConfiguration : IEntityTypeConfiguration<PromotionRule>
{
    public void Configure(EntityTypeBuilder<PromotionRule> builder)
    {
        builder.ToTable("promotion_rules");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.PromotionId)
            .HasColumnName("promotion_id")
            .IsRequired();

        builder.Property(r => r.RuleType)
            .HasColumnName("rule_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.Operator)
            .HasColumnName("operator")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.Value)
            .HasColumnName("value")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(r => r.Logic)
            .HasColumnName("logic")
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.HasIndex(r => r.PromotionId)
            .HasDatabaseName("ix_promotion_rules_promotion_id");
    }
}
