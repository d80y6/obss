using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ServiceQualification.Domain.Entities;

namespace Obss.ServiceQualification.Infrastructure.Persistence.Configurations;

public class AlternateServiceProposalConfiguration : IEntityTypeConfiguration<AlternateServiceProposal>
{
    public void Configure(EntityTypeBuilder<AlternateServiceProposal> builder)
    {
        builder.ToTable("service_qualification_item_alternatives");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ServiceId).IsRequired();
        builder.Property(x => x.ServiceName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ResultType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(x => x.EstimatedInstallDate);
        builder.Property(x => x.GuaranteedUntil);
    }
}
