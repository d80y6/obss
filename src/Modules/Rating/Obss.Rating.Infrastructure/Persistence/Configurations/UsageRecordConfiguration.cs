using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Rating.Domain.Entities;
using Obss.Rating.Domain.ValueObjects;

namespace Obss.Rating.Infrastructure.Persistence.Configurations;

public sealed class UsageRecordConfiguration : IEntityTypeConfiguration<UsageRecord>
{
    public void Configure(EntityTypeBuilder<UsageRecord> builder)
    {
        builder.ToTable("usage_records");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.SubscriptionId)
            .HasColumnName("subscription_id")
            .IsRequired();

        builder.Property(r => r.ServiceId)
            .HasColumnName("service_id")
            .IsRequired();

        builder.Property(r => r.RecordType)
            .HasColumnName("record_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.UsageType)
            .HasColumnName("usage_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.StartTime)
            .HasColumnName("start_time")
            .IsRequired();

        builder.Property(r => r.EndTime)
            .HasColumnName("end_time")
            .IsRequired();

        builder.Property(r => r.Duration)
            .HasColumnName("duration")
            .IsRequired();

        builder.Property(r => r.Volume)
            .HasColumnName("volume")
            .IsRequired();

        builder.Property(r => r.SourceIdentifier)
            .HasColumnName("source_identifier")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.DestinationIdentifier)
            .HasColumnName("destination_identifier")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.RatedAmount)
            .HasColumnName("rated_amount")
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(r => r.RatingRuleId)
            .HasColumnName("rating_rule_id");

        builder.Property(r => r.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(r => r.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(1000);

        builder.Property(r => r.RecordedAt)
            .HasColumnName("recorded_at")
            .IsRequired();

        builder.Property(r => r.RatedAt)
            .HasColumnName("rated_at");

        builder.HasIndex(r => r.Status)
            .HasDatabaseName("ix_usage_records_status");

        builder.HasIndex(r => r.SubscriptionId)
            .HasDatabaseName("ix_usage_records_subscription_id");

        builder.HasIndex(r => r.RecordedAt)
            .HasDatabaseName("ix_usage_records_recorded_at");

        builder.HasIndex(r => r.TenantId)
            .HasDatabaseName("ix_usage_records_tenant_id");

        builder.HasIndex(r => new { r.Status, r.RecordedAt })
            .HasDatabaseName("ix_usage_records_status_recorded_at");
    }
}
