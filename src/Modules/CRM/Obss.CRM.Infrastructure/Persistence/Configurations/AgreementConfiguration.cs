using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.CRM.Domain.Entities;

namespace Obss.CRM.Infrastructure.Persistence.Configurations;

public sealed class AgreementConfiguration : IEntityTypeConfiguration<Agreement>
{
    public void Configure(EntityTypeBuilder<Agreement> builder)
    {
        builder.ToTable("agreements");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(a => a.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(a => a.AgreementType).HasColumnName("agreement_type").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(a => a.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(a => a.ValidFrom).HasColumnName("valid_from");
        builder.Property(a => a.ValidUntil).HasColumnName("valid_until");
        builder.Property(a => a.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(a => a.SignedAt).HasColumnName("signed_at");
        builder.Property(a => a.SignedBy).HasColumnName("signed_by").HasMaxLength(100);
        builder.Property(a => a.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(a => a.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(a => a.CustomerId).HasDatabaseName("ix_agreements_customer_id");
    }
}
