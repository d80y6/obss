using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Billing.Domain.Entities;

namespace Obss.Billing.Infrastructure.Persistence.Configurations;

public sealed class BillingJobConfiguration : IEntityTypeConfiguration<BillingJob>
{
    public void Configure(EntityTypeBuilder<BillingJob> builder)
    {
        builder.ToTable("billing_jobs");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Id)
            .ValueGeneratedNever();

        builder.Property(j => j.JobType)
            .HasColumnName("job_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(j => j.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(j => j.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(2000);

        builder.Property(j => j.TotalProcessed)
            .HasColumnName("total_processed")
            .IsRequired();

        builder.Property(j => j.TotalErrors)
            .HasColumnName("total_errors")
            .IsRequired();

        builder.Property(j => j.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(j => j.StartedAt)
            .HasColumnName("started_at");

        builder.Property(j => j.CompletedAt)
            .HasColumnName("completed_at");

        builder.HasIndex(j => j.Status)
            .HasDatabaseName("ix_billing_jobs_status");

        builder.HasIndex(j => j.CreatedAt)
            .HasDatabaseName("ix_billing_jobs_created_at");
    }
}
