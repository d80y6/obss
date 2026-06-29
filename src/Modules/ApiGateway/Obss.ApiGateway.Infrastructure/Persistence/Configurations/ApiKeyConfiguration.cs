using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ApiGateway.Domain.Entities;

namespace Obss.ApiGateway.Infrastructure.Persistence.Configurations;

public sealed class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("api_keys");

        builder.HasKey(k => k.Id);

        builder.Property(k => k.Id)
            .ValueGeneratedNever();

        builder.Property(k => k.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(k => k.PartnerId)
            .HasColumnName("partner_id");

        builder.Property(k => k.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(k => k.Key)
            .HasColumnName("key")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(k => k.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(k => k.Permissions)
            .HasColumnName("permissions")
            .HasColumnType("jsonb")
            .HasConversion(new ListStringConverter())
            .IsRequired();

        builder.Property(k => k.AllowedIPs)
            .HasColumnName("allowed_ips")
            .HasColumnType("jsonb")
            .HasConversion(new ListStringConverter())
            .IsRequired();

        builder.Property(k => k.RateLimitPerMinute)
            .HasColumnName("rate_limit_per_minute")
            .IsRequired();

        builder.Property(k => k.ExpiresAt)
            .HasColumnName("expires_at");

        builder.Property(k => k.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(k => k.RevokedAt)
            .HasColumnName("revoked_at");

        builder.HasIndex(k => k.Key)
            .HasDatabaseName("ix_api_keys_key")
            .IsUnique();

        builder.HasIndex(k => k.PartnerId)
            .HasDatabaseName("ix_api_keys_partner_id");
    }
}
