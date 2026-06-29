using Microsoft.EntityFrameworkCore;
using Obss.ServiceInventory.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ServiceInventory.Infrastructure.Persistence;

public class ServiceDbContext : EfDbContext
{
    public ServiceDbContext(
        DbContextOptions<ServiceDbContext> options,
        ICurrentTenant currentTenant,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options, currentTenant, domainEventDispatcher)
    {
    }

    public DbSet<Service> Services => Set<Service>();
    public DbSet<ServiceResource> ServiceResources => Set<ServiceResource>();
    public DbSet<ServiceTopology> ServiceTopologies => Set<ServiceTopology>();
    public DbSet<TopologyLink> TopologyLinks => Set<TopologyLink>();
    public DbSet<ResourceDiscoveryJob> ResourceDiscoveryJobs => Set<ResourceDiscoveryJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServiceDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
