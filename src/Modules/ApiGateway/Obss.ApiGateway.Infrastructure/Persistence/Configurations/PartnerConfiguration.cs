using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ApiGateway.Domain.Entities;

namespace Obss.ApiGateway.Infrastructure.Persistence.Configurations;

public sealed class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.ToTable("partners");

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

        builder.Property(p => p.ContactName)
            .HasColumnName("contact_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.ContactEmail)
            .HasColumnName("contact_email")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(p => p.AllowedIPs)
            .HasColumnName("allowed_ips")
            .HasColumnType("jsonb")
            .HasConversion(new ListStringConverter())
            .IsRequired();

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(p => p.SlaLevel)
            .HasColumnName("sla_level")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.MaxRequestsPerDay)
            .HasColumnName("max_requests_per_day")
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasMany(p => p.ApiKeys)
            .WithOne()
            .HasForeignKey(k => k.PartnerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(p => p.Name)
            .HasDatabaseName("ix_partners_name")
            .IsUnique();
    }
}
