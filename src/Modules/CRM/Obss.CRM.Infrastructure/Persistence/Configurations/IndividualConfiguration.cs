using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.CRM.Domain.Entities;

namespace Obss.CRM.Infrastructure.Persistence.Configurations;

public sealed class IndividualConfiguration : IEntityTypeConfiguration<Individual>
{
    public void Configure(EntityTypeBuilder<Individual> builder)
    {
        builder.ToTable("individuals");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
        builder.Property(i => i.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
        builder.Property(i => i.MiddleName).HasColumnName("middle_name").HasMaxLength(100);
        builder.Property(i => i.Salutation).HasColumnName("salutation").HasMaxLength(20);
        builder.Property(i => i.Title).HasColumnName("title").HasMaxLength(50);
        builder.Property(i => i.BirthDate).HasColumnName("birth_date");
        builder.Property(i => i.Nationality).HasColumnName("nationality").HasMaxLength(3);
        builder.Property(i => i.Gender).HasColumnName("gender").HasMaxLength(20);
        builder.Property(i => i.KycStatus).HasColumnName("kyc_status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(i => i.KycVerifiedAt).HasColumnName("kyc_verified_at");
        builder.Property(i => i.KycVerifiedBy).HasColumnName("kyc_verified_by").HasMaxLength(100);
        builder.Property(i => i.RiskRating).HasColumnName("risk_rating").HasConversion<string>().HasMaxLength(10).IsRequired();

        builder.HasMany(i => i.Documents)
            .WithOne()
            .HasForeignKey(d => d.IndividualId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(i => i.Documents).AutoInclude();
    }
}
