using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Collections.Domain.Entities;

namespace Obss.Collections.Infrastructure.Persistence.Configurations;

public sealed class CollectionActionConfiguration : IEntityTypeConfiguration<CollectionAction>
{
    public void Configure(EntityTypeBuilder<CollectionAction> builder)
    {
        builder.ToTable("collection_actions");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.CollectionCaseId)
            .HasColumnName("collection_case_id")
            .IsRequired();

        builder.Property(a => a.ActionType)
            .HasColumnName("action_type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(a => a.DunningLevel)
            .HasColumnName("dunning_level")
            .IsRequired();

        builder.Property(a => a.Description)
            .HasColumnName("description")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(a => a.PerformedAt)
            .HasColumnName("performed_at")
            .IsRequired();

        builder.Property(a => a.PerformedBy)
            .HasColumnName("performed_by")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.NextActionDate)
            .HasColumnName("next_action_date");

        builder.Property(a => a.IsCompleted)
            .HasColumnName("is_completed")
            .IsRequired();

        builder.HasIndex(a => a.CollectionCaseId)
            .HasDatabaseName("ix_collection_actions_case_id");

        builder.HasIndex(a => a.ActionType)
            .HasDatabaseName("ix_collection_actions_action_type");
    }
}
