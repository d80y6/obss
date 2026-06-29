using Microsoft.EntityFrameworkCore;
using Obss.ModuleTemplate.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ModuleTemplate.Infrastructure.Persistence;

public class SampleDbContext : EfDbContext
{
    public SampleDbContext(
        DbContextOptions<SampleDbContext> options,
        ICurrentTenant currentTenant,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options, currentTenant, domainEventDispatcher)
    {
    }

    public DbSet<SampleAggregate> SampleAggregates => Set<SampleAggregate>();
    public DbSet<SampleEntity> SampleEntities => Set<SampleEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SampleDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
