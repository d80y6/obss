using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Subscriptions.Domain.Entities;

namespace Obss.Subscriptions.Infrastructure.Persistence.Configurations;

public sealed class RealizingServiceConfiguration : IEntityTypeConfiguration<RealizingService>
{
    public void Configure(EntityTypeBuilder<RealizingService> builder)
    {
        builder.ToTable("realizing_services");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.Property(s => s.ServiceId).HasColumnName("service_id").IsRequired();
        builder.Property(s => s.ServiceType).HasColumnName("service_type").HasMaxLength(100).IsRequired();
        builder.Property(s => s.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
    }
}
