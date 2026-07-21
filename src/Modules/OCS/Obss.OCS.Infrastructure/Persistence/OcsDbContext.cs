using Microsoft.EntityFrameworkCore;
using Obss.OCS.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.OCS.Infrastructure.Persistence;

public class OcsDbContext : EfDbContext
{
    public OcsDbContext(
        DbContextOptions<OcsDbContext> options,
        ICurrentTenant currentTenant,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options, currentTenant, domainEventDispatcher)
    {
    }

    public DbSet<Balance> Balances => Set<Balance>();
    public DbSet<CreditPool> CreditPools => Set<CreditPool>();
    public DbSet<OcsTransaction> OcsTransactions => Set<OcsTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OcsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
