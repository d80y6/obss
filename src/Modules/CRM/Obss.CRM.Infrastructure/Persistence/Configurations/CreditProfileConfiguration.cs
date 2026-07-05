using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.CRM.Domain.Entities;

namespace Obss.CRM.Infrastructure.Persistence.Configurations;

public sealed class CreditProfileConfiguration : IEntityTypeConfiguration<CreditProfile>
{
    public void Configure(EntityTypeBuilder<CreditProfile> builder)
    {
        builder.ToTable("customer_credit_profiles");

        builder.HasKey(cp => cp.Id);
        builder.Property(cp => cp.Id).ValueGeneratedNever();

        builder.Property(cp => cp.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(cp => cp.Score).HasColumnName("score").IsRequired();
        builder.Property(cp => cp.ScoreType).HasColumnName("score_type").HasMaxLength(50).IsRequired();
        builder.Property(cp => cp.RiskRating).HasColumnName("risk_rating").HasMaxLength(20);

        builder.OwnsOne(cp => cp.ValidFor, vf =>
        {
            vf.Property(p => p.StartDateTime).HasColumnName("valid_from");
            vf.Property(p => p.EndDateTime).HasColumnName("valid_until");
        });
    }
}
