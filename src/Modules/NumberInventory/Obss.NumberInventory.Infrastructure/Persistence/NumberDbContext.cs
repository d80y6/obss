using Microsoft.EntityFrameworkCore;
using Obss.NumberInventory.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.NumberInventory.Infrastructure.Persistence;

public class NumberDbContext : EfDbContext
{
    public NumberDbContext(
        DbContextOptions<NumberDbContext> options,
        ICurrentTenant currentTenant,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options, currentTenant, domainEventDispatcher)
    {
    }

    public DbSet<TelephoneNumber> TelephoneNumbers => Set<TelephoneNumber>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NumberDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
