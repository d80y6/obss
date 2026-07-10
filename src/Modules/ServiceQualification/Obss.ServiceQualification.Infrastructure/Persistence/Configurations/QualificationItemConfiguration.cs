using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ServiceQualification.Domain.Entities;

namespace Obss.ServiceQualification.Infrastructure.Persistence.Configurations;

public class QualificationItemConfiguration : IEntityTypeConfiguration<QualificationItem>
{
    public void Configure(EntityTypeBuilder<QualificationItem> builder)
    {
        builder.ToTable("service_qualification_items");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ServiceId).IsRequired();
        builder.Property(x => x.ServiceName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ResultType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(x => x.State)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(x => x.EstimatedInstallDate);
        builder.Property(x => x.EstimatedCompletionDate);
        builder.Property(x => x.EligibilityUnavailableReason).HasMaxLength(500);

        builder.HasMany(x => x.AlternateProposals)
            .WithOne()
            .HasForeignKey("item_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
