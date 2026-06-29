using Microsoft.EntityFrameworkCore;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.NetworkInventory.Infrastructure.Persistence;

public class NetworkDbContext : EfDbContext
{
    public NetworkDbContext(
        DbContextOptions<NetworkDbContext> options,
        ICurrentTenant currentTenant,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options, currentTenant, domainEventDispatcher)
    {
    }

    public DbSet<NetworkElement> NetworkElements => Set<NetworkElement>();
    public DbSet<NetworkInterface> Interfaces => Set<NetworkInterface>();
    public DbSet<NetworkElementIpAddress> IpAddresses => Set<NetworkElementIpAddress>();
    public DbSet<Subnet> Subnets => Set<Subnet>();
    public DbSet<VLAN> VLANs => Set<VLAN>();
    public DbSet<OLT> OLTs => Set<OLT>();
    public DbSet<PONPort> PONPorts => Set<PONPort>();
    public DbSet<FiberCable> FiberCables => Set<FiberCable>();
    public DbSet<ConnectivityLink> ConnectivityLinks => Set<ConnectivityLink>();
    public DbSet<CapacityRecord> CapacityRecords => Set<CapacityRecord>();
    public DbSet<TopologyMap> TopologyMaps => Set<TopologyMap>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NetworkDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
