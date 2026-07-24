using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.AAA.Domain.Entities;

namespace Obss.AAA.Infrastructure.Persistence.Configurations;

public sealed class AaaAuditLogConfiguration : IEntityTypeConfiguration<AaaAuditLog>
{
    public void Configure(EntityTypeBuilder<AaaAuditLog> builder)
    {
        builder.ToTable("aaa_audit_logs");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.TenantId).HasColumnName("tenant_id").HasMaxLength(100).IsRequired();
        builder.Property(l => l.EventType).HasColumnName("event_type").HasMaxLength(50).IsRequired();
        builder.Property(l => l.Username).HasColumnName("username").HasMaxLength(200);
        builder.Property(l => l.NasId).HasColumnName("nas_id");
        builder.Property(l => l.NasIpAddress).HasColumnName("nas_ip_address").HasMaxLength(45);
        builder.Property(l => l.Detail).HasColumnName("detail").HasColumnType("jsonb");
        builder.Property(l => l.Timestamp).HasColumnName("timestamp").IsRequired();

        builder.HasIndex(l => l.Timestamp).HasDatabaseName("ix_aaa_audit_logs_timestamp");
        builder.HasIndex(l => l.EventType).HasDatabaseName("ix_aaa_audit_logs_event_type");
        builder.HasIndex(l => l.NasId).HasDatabaseName("ix_aaa_audit_logs_nas_id");
        builder.HasIndex(l => l.Username).HasDatabaseName("ix_aaa_audit_logs_username");
    }
}
