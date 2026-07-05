using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.CRM.Domain.Entities;

namespace Obss.CRM.Infrastructure.Persistence.Configurations;

public sealed class IdentityDocumentConfiguration : IEntityTypeConfiguration<IdentityDocument>
{
    public void Configure(EntityTypeBuilder<IdentityDocument> builder)
    {
        builder.ToTable("individual_identity_documents");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.IndividualId).HasColumnName("individual_id").IsRequired();
        builder.Property(d => d.DocumentType).HasColumnName("document_type").HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(d => d.DocumentNumber).HasColumnName("document_number").HasMaxLength(100).IsRequired();
        builder.Property(d => d.IssuingAuthority).HasColumnName("issuing_authority").HasMaxLength(200);
        builder.Property(d => d.IssuingCountry).HasColumnName("issuing_country").HasMaxLength(3);
        builder.Property(d => d.IssuedDate).HasColumnName("issued_date");
        builder.Property(d => d.ExpiryDate).HasColumnName("expiry_date");
        builder.Property(d => d.IsVerified).HasColumnName("is_verified").IsRequired();
    }
}
