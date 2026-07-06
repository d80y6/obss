using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ServiceCatalog.Domain.Entities;

namespace Obss.ServiceCatalog.Infrastructure.Persistence.Configurations;

public sealed class ServiceCandidateConfiguration : IEntityTypeConfiguration<ServiceCandidate>
{
    public void Configure(EntityTypeBuilder<ServiceCandidate> builder)
    {
        builder.ToTable("service_candidates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        builder.Property(x => x.LifecycleStatus)
            .HasColumnName("lifecycle_status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Version)
            .HasColumnName("version")
            .IsRequired();

        builder.Property(x => x.ValidFrom)
            .HasColumnName("valid_from");

        builder.Property(x => x.ValidTo)
            .HasColumnName("valid_to");

        builder.Property(x => x.ServiceSpecificationId)
            .HasColumnName("service_specification_id");

        builder.Property(x => x.BaseCandidateId)
            .HasColumnName("base_candidate_id");

        builder.Property(x => x.FeatureSpecification)
            .HasColumnName("feature_specification")
            .HasColumnType("jsonb");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.TenantId).HasDatabaseName("ix_service_candidates_tenant_id");
        builder.HasIndex(x => x.ServiceSpecificationId).HasDatabaseName("ix_service_candidates_service_specification_id");
        builder.HasIndex(x => x.LifecycleStatus).HasDatabaseName("ix_service_candidates_lifecycle_status");
    }
}
