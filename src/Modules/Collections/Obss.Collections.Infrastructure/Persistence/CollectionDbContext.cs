using Microsoft.EntityFrameworkCore;
using Obss.Collections.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Collections.Infrastructure.Persistence;

public class CollectionDbContext : EfDbContext
{
    public CollectionDbContext(
        DbContextOptions<CollectionDbContext> options,
        ICurrentTenant currentTenant,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options, currentTenant, domainEventDispatcher)
    {
    }

    public DbSet<CollectionCase> CollectionCases => Set<CollectionCase>();
    public DbSet<CollectionAction> CollectionActions => Set<CollectionAction>();
    public DbSet<PaymentArrangement> PaymentArrangements => Set<PaymentArrangement>();
    public DbSet<Installment> Installments => Set<Installment>();
    public DbSet<DunningPolicy> DunningPolicies => Set<DunningPolicy>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CollectionDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
