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
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OcsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Balance>().Where(e => e.State is EntityState.Modified))
        {
            var orig = entry.Property("ConcurrencyStamp").OriginalValue is uint u ? u : 0u;
            entry.Property("ConcurrencyStamp").CurrentValue = orig + 1;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
