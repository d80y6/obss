using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Obss.CRM.Domain.Entities;
using Obss.CRM.Domain.ValueObjects;

namespace Obss.CRM.Infrastructure.Persistence.Configurations;

public sealed class CustomerSegmentConfiguration : IEntityTypeConfiguration<CustomerSegment>
{
    private static readonly JsonSerializerOptions JsonOptions = new();

    public void Configure(EntityTypeBuilder<CustomerSegment> builder)
    {
        builder.ToTable("customer_segments");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        var criteriaConverter = new ValueConverter<SegmentCriteria, string>(
            v => JsonSerializer.Serialize(v, JsonOptions),
            v => JsonSerializer.Deserialize<SegmentCriteria>(v, JsonOptions) ?? new SegmentCriteria(new List<RuleGroup>()));

        var criteriaComparer = new ValueComparer<SegmentCriteria>(
            (c1, c2) => JsonSerializer.Serialize(c1, JsonOptions) == JsonSerializer.Serialize(c2, JsonOptions),
            c => c.GetHashCode(),
            c => JsonSerializer.Deserialize<SegmentCriteria>(JsonSerializer.Serialize(c, JsonOptions), JsonOptions) ?? new SegmentCriteria(new List<RuleGroup>()));

        builder.Property(s => s.Criteria)
            .HasColumnName("criteria")
            .HasColumnType("jsonb")
            .HasConversion(criteriaConverter, criteriaComparer)
            .IsRequired();

        builder.Property(s => s.Priority)
            .HasColumnName("priority")
            .IsRequired();

        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasMany(s => s.Assignments)
            .WithOne(a => a.Segment)
            .HasForeignKey(a => a.SegmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.TenantId)
            .HasDatabaseName("ix_customer_segments_tenant_id");

        builder.HasIndex(s => s.Name)
            .HasDatabaseName("ix_customer_segments_name");

        builder.HasIndex(s => s.Priority)
            .HasDatabaseName("ix_customer_segments_priority");

        builder.Navigation(s => s.Assignments)
            .AutoInclude();
    }
}
