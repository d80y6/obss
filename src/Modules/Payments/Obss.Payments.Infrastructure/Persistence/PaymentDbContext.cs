using Microsoft.EntityFrameworkCore;
using Obss.Payments.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Payments.Infrastructure.Persistence;

public class PaymentDbContext : EfDbContext
{
    public PaymentDbContext(
        DbContextOptions<PaymentDbContext> options,
        ICurrentTenant currentTenant,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options, currentTenant, domainEventDispatcher)
    {
    }

    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<Refund> Refunds => Set<Refund>();
    public DbSet<PaymentAllocation> PaymentAllocations => Set<PaymentAllocation>();
    public DbSet<PaymentGateway> PaymentGateways => Set<PaymentGateway>();
    public DbSet<PaymentReconciliation> PaymentReconciliations => Set<PaymentReconciliation>();
    public DbSet<ReconciliationItem> ReconciliationItems => Set<ReconciliationItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
