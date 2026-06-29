using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.CRM.Domain.Entities;

namespace Obss.CRM.Infrastructure.Persistence.Configurations;

public sealed class CustomerSegmentAssignmentConfiguration : IEntityTypeConfiguration<CustomerSegmentAssignment>
{
    public void Configure(EntityTypeBuilder<CustomerSegmentAssignment> builder)
    {
        builder.ToTable("customer_segment_assignments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(a => a.SegmentId)
            .HasColumnName("segment_id")
            .IsRequired();

        builder.Property(a => a.AssignedAt)
            .HasColumnName("assigned_at")
            .IsRequired();

        builder.Property(a => a.AssignedBy)
            .HasColumnName("assigned_by")
            .IsRequired();

        builder.Property(a => a.IsAutoAssigned)
            .HasColumnName("is_auto_assigned")
            .IsRequired();

        builder.HasIndex(a => a.CustomerId)
            .HasDatabaseName("ix_customer_segment_assignments_customer_id");

        builder.HasIndex(a => a.SegmentId)
            .HasDatabaseName("ix_customer_segment_assignments_segment_id");

        builder.HasIndex(a => new { a.SegmentId, a.CustomerId })
            .IsUnique()
            .HasDatabaseName("ix_customer_segment_assignments_segment_customer");
    }
}
