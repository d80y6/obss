using Microsoft.EntityFrameworkCore;
using Obss.ServiceCatalog.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ServiceCatalog.Infrastructure.Persistence;

public class ServiceCatalogDbContext : EfDbContext
{
    public ServiceCatalogDbContext(
        DbContextOptions<ServiceCatalogDbContext> options,
        ICurrentTenant currentTenant,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options, currentTenant, domainEventDispatcher)
    {
    }

    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<ServiceCandidate> ServiceCandidates => Set<ServiceCandidate>();
    public DbSet<ServiceSpecification> ServiceSpecifications => Set<ServiceSpecification>();
    public DbSet<ServiceSpecCharacteristic> ServiceSpecCharacteristics => Set<ServiceSpecCharacteristic>();
    public DbSet<ServiceSpecCharValue> ServiceSpecCharValues => Set<ServiceSpecCharValue>();
    public DbSet<ServiceSpecRelationship> ServiceSpecRelationships => Set<ServiceSpecRelationship>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServiceCatalogDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
