using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.CRM.Domain.Entities;

namespace Obss.CRM.Infrastructure.Persistence.Configurations;

public sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("organizations");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();

        builder.Property(o => o.TradingName).HasColumnName("trading_name").HasMaxLength(200).IsRequired();
        builder.Property(o => o.CompanyType).HasColumnName("company_type").HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(o => o.Industry).HasColumnName("industry").HasMaxLength(100);
        builder.Property(o => o.RegistrationNumber).HasColumnName("registration_number").HasMaxLength(100);
        builder.Property(o => o.TaxNumber).HasColumnName("tax_number").HasMaxLength(50);
        builder.Property(o => o.CountryOfRegistration).HasColumnName("country_of_registration").HasMaxLength(3);
        builder.Property(o => o.KycStatus).HasColumnName("kyc_status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(o => o.KycVerifiedAt).HasColumnName("kyc_verified_at");
        builder.Property(o => o.KycVerifiedBy).HasColumnName("kyc_verified_by").HasMaxLength(100);
    }
}
