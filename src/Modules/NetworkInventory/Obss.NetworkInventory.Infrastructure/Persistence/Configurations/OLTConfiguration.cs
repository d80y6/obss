using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.NetworkInventory.Domain.Entities;

namespace Obss.NetworkInventory.Infrastructure.Persistence.Configurations;

public sealed class OLTConfiguration : IEntityTypeConfiguration<OLT>
{
    public void Configure(EntityTypeBuilder<OLT> builder)
    {
        builder.Property(o => o.MaxPONPorts)
            .HasColumnName("max_pon_ports")
            .IsRequired();

        builder.Property(o => o.UsedPONPorts)
            .HasColumnName("used_pon_ports")
            .IsRequired();

        builder.Property(o => o.MaxONTPerPort)
            .HasColumnName("max_ont_per_port")
            .IsRequired();

        builder.Property(o => o.MaxBandwidth)
            .HasColumnName("max_bandwidth")
            .IsRequired();

        builder.HasMany(o => o.PONPorts)
            .WithOne()
            .HasForeignKey(p => p.OLTId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.PONPorts)
            .AutoInclude();
    }
}
