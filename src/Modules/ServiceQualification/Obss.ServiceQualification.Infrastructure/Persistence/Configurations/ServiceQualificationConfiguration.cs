using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ServiceQualification.Domain.Entities;

namespace Obss.ServiceQualification.Infrastructure.Persistence.Configurations;

public class ServiceQualificationConfiguration : IEntityTypeConfiguration<Domain.Entities.ServiceQualification>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.ServiceQualification> builder)
    {
        builder.ToTable("service_qualifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomerId).IsRequired();
        builder.Property(x => x.State)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(x => x.RequestedDate).IsRequired();
        builder.Property(x => x.ExpirationDate);
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.OwnsOne(x => x.Address, address =>
        {
            address.Property(a => a.Street).HasColumnName("address_street").HasMaxLength(200).IsRequired();
            address.Property(a => a.City).HasColumnName("address_city").HasMaxLength(100).IsRequired();
            address.Property(a => a.State).HasColumnName("address_state").HasMaxLength(50);
            address.Property(a => a.PostalCode).HasColumnName("address_postal_code").HasMaxLength(20);
            address.Property(a => a.Country).HasColumnName("address_country").HasMaxLength(50).IsRequired();
        });

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey("qualification_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(x => x.DomainEvents);
    }
}
