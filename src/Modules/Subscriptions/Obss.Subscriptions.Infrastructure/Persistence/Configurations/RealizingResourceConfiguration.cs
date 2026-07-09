using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Subscriptions.Domain.Entities;

namespace Obss.Subscriptions.Infrastructure.Persistence.Configurations;

public sealed class RealizingResourceConfiguration : IEntityTypeConfiguration<RealizingResource>
{
    public void Configure(EntityTypeBuilder<RealizingResource> builder)
    {
        builder.ToTable("realizing_resources");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.ResourceId).HasColumnName("resource_id").IsRequired();
        builder.Property(r => r.ResourceType).HasColumnName("resource_type").HasMaxLength(100).IsRequired();
        builder.Property(r => r.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
    }
}
