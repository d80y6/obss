using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ServiceInventory.Domain.Entities;

namespace Obss.ServiceInventory.Infrastructure.Persistence.Configurations;

public sealed class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("services");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(s => s.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(s => s.SubscriptionId)
            .HasColumnName("subscription_id")
            .IsRequired();

        builder.Property(s => s.ServiceType)
            .HasColumnName("service_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.ServiceIdentifier)
            .HasColumnName("service_identifier")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.ActivationDate)
            .HasColumnName("activation_date");

        builder.Property(s => s.SuspendedAt)
            .HasColumnName("suspended_at");

        builder.Property(s => s.DecommissionedAt)
            .HasColumnName("decommissioned_at");

        builder.Property(s => s.Configuration)
            .HasColumnName("configuration")
            .HasColumnType("jsonb");

        builder.Property(s => s.Location)
            .HasColumnName("location")
            .HasMaxLength(500);

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasMany(s => s.Resources)
            .WithOne()
            .HasForeignKey(r => r.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.CustomerId)
            .HasDatabaseName("ix_services_customer_id");

        builder.HasIndex(s => s.SubscriptionId)
            .HasDatabaseName("ix_services_subscription_id");

        builder.HasIndex(s => s.ServiceIdentifier)
            .HasDatabaseName("ix_services_service_identifier")
            .IsUnique();

        builder.HasIndex(s => s.Status)
            .HasDatabaseName("ix_services_status");

        builder.HasIndex(s => s.ServiceType)
            .HasDatabaseName("ix_services_service_type");

        builder.HasIndex(s => new { s.CustomerId, s.Status })
            .HasDatabaseName("ix_services_customer_id_status");

        builder.Navigation(s => s.Resources)
            .AutoInclude();
    }
}
