using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ModuleTemplate.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ModuleTemplate.Infrastructure.Persistence.Configurations;

public sealed class SampleAggregateConfiguration : IEntityTypeConfiguration<SampleAggregate>
{
    public void Configure(EntityTypeBuilder<SampleAggregate> builder)
    {
        builder.ToTable("sample_aggregates");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(s => s.Name)
            .HasDatabaseName("ix_sample_aggregates_name");

        builder.HasIndex("TenantId", "Name")
            .HasDatabaseName("ix_sample_aggregates_tenant_id_name")
            .IsUnique();
    }
}

public sealed class SampleEntityConfiguration : IEntityTypeConfiguration<SampleEntity>
{
    public void Configure(EntityTypeBuilder<SampleEntity> builder)
    {
        builder.ToTable("sample_entities");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.AggregateId)
            .HasColumnName("aggregate_id")
            .IsRequired();

        builder.Property(e => e.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(e => e.Code)
            .HasDatabaseName("ix_sample_entities_code")
            .IsUnique();
    }
}
