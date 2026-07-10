using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ServiceQualification.Domain.Entities;
using Obss.ServiceQualification.Domain.ValueObjects;

namespace Obss.ServiceQualification.Infrastructure.Persistence.Configurations;

public class CoverageAreaConfiguration : IEntityTypeConfiguration<CoverageArea>
{
    public void Configure(EntityTypeBuilder<CoverageArea> builder)
    {
        builder.ToTable("coverage_areas");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.City).HasMaxLength(100).IsRequired();
        builder.Property(x => x.State).HasMaxLength(50);
        builder.Property(x => x.StreetFrom).HasMaxLength(200);
        builder.Property(x => x.StreetTo).HasMaxLength(200);
        builder.Property(x => x.PostalCode).HasMaxLength(20);

        builder.OwnsMany(x => x.AvailableServices, cs =>
        {
            cs.ToTable("coverage_area_services");
            cs.WithOwner().HasForeignKey("coverage_area_id");
            cs.Property<Guid>("Id");
            cs.HasKey("Id");

            cs.Property(c => c.ServiceName).HasMaxLength(200).IsRequired();
            cs.Property(c => c.SpeedMbps);
            cs.Property(c => c.Technology).HasMaxLength(50).IsRequired();
            cs.Property(c => c.MonthlyPrice).HasColumnType("decimal(18,2)");
            cs.Property(c => c.IsActive).IsRequired();
        });

        // Seed data for sample cities
        builder.HasData(
            new
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                City = "Sana'a",
                State = (string?)null,
                StreetFrom = (string?)null,
                StreetTo = (string?)null,
                PostalCode = (string?)null
            },
            new
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                City = "Aden",
                State = (string?)null,
                StreetFrom = (string?)null,
                StreetTo = (string?)null,
                PostalCode = (string?)null
            },
            new
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                City = "Taiz",
                State = (string?)null,
                StreetFrom = (string?)null,
                StreetTo = (string?)null,
                PostalCode = (string?)null
            }
        );
    }
}
