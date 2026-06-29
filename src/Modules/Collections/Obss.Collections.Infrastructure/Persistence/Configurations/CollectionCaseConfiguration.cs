using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Collections.Domain.Entities;

namespace Obss.Collections.Infrastructure.Persistence.Configurations;

public sealed class CollectionCaseConfiguration : IEntityTypeConfiguration<CollectionCase>
{
    public void Configure(EntityTypeBuilder<CollectionCase> builder)
    {
        builder.ToTable("collection_cases");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(c => c.CustomerName)
            .HasColumnName("customer_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.TotalOverdueAmount)
            .HasColumnName("total_overdue_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(c => c.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(c => c.CurrentDunningLevel)
            .HasColumnName("current_dunning_level")
            .IsRequired();

        builder.Property(c => c.OpenedAt)
            .HasColumnName("opened_at")
            .IsRequired();

        builder.Property(c => c.LastActionAt)
            .HasColumnName("last_action_at");

        builder.Property(c => c.ResolvedAt)
            .HasColumnName("resolved_at");

        builder.Property(c => c.AssignedTo)
            .HasColumnName("assigned_to")
            .HasMaxLength(100);

        builder.Property(c => c.Notes)
            .HasColumnName("notes")
            .HasMaxLength(4000);

        builder.HasMany(c => c.Actions)
            .WithOne()
            .HasForeignKey(a => a.CollectionCaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.PaymentArrangements)
            .WithOne()
            .HasForeignKey(pa => pa.CollectionCaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Actions)
            .AutoInclude();

        builder.Navigation(c => c.PaymentArrangements)
            .AutoInclude();

        builder.HasIndex(c => c.CustomerId)
            .HasDatabaseName("ix_collection_cases_customer_id");

        builder.HasIndex(c => c.Status)
            .HasDatabaseName("ix_collection_cases_status");

        builder.HasIndex(c => c.CurrentDunningLevel)
            .HasDatabaseName("ix_collection_cases_dunning_level");

        builder.HasIndex(c => c.TenantId)
            .HasDatabaseName("ix_collection_cases_tenant_id");
    }
}
