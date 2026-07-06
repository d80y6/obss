using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ServiceCatalog.Domain.Entities;

namespace Obss.ServiceCatalog.Infrastructure.Persistence.Configurations;

public sealed class ServiceSpecRelationshipConfiguration : IEntityTypeConfiguration<ServiceSpecRelationship>
{
    public void Configure(EntityTypeBuilder<ServiceSpecRelationship> builder)
    {
        builder.ToTable("service_spec_relationships");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.ServiceSpecificationId)
            .HasColumnName("service_specification_id")
            .IsRequired();

        builder.Property(x => x.TargetSpecificationId)
            .HasColumnName("target_specification_id")
            .IsRequired();

        builder.Property(x => x.RelationshipType)
            .HasColumnName("relationship_type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Role)
            .HasColumnName("role")
            .HasMaxLength(100);

        builder.Property(x => x.ValidFrom)
            .HasColumnName("valid_from");

        builder.Property(x => x.ValidTo)
            .HasColumnName("valid_to");
    }
}
