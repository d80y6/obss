using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Rating.Domain.Entities;

namespace Obss.Rating.Infrastructure.Persistence.Configurations;

public sealed class CdrRecordConfiguration : IEntityTypeConfiguration<CdrRecord>
{
    public void Configure(EntityTypeBuilder<CdrRecord> builder)
    {
        builder.ToTable("cdr_records");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.TenantId).HasColumnName("tenant_id").HasMaxLength(100).IsRequired();

        builder.Property(r => r.CorrelationId).HasColumnName("correlation_id").HasMaxLength(200).IsRequired();
        builder.Property(r => r.Vendor).HasColumnName("vendor").HasMaxLength(50).IsRequired();
        builder.Property(r => r.SourceIp).HasColumnName("source_ip").HasMaxLength(45).IsRequired();
        builder.Property(r => r.Payload).HasColumnName("payload").HasColumnType("text").IsRequired();
        builder.Property(r => r.NormalizedData).HasColumnName("normalized_data").HasColumnType("text");
        builder.Property(r => r.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(r => r.ErrorReason).HasColumnName("error_reason").HasMaxLength(1000);
        builder.Property(r => r.ReceivedAt).HasColumnName("received_at").IsRequired();
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(r => r.CorrelationId).IsUnique().HasDatabaseName("ix_cdr_records_correlation_id");
        builder.HasIndex(r => r.Vendor).HasDatabaseName("ix_cdr_records_vendor");
        builder.HasIndex(r => r.Status).HasDatabaseName("ix_cdr_records_status");
        builder.HasIndex(r => r.ReceivedAt).HasDatabaseName("ix_cdr_records_received_at");
    }
}
