using Microsoft.EntityFrameworkCore;
using Obss.Orders.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Orders.Infrastructure.Persistence;

public class OrderDbContext : EfDbContext
{
    public OrderDbContext(
        DbContextOptions<OrderDbContext> options,
        ICurrentTenant currentTenant,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options, currentTenant, domainEventDispatcher)
    {
    }

    public DbSet<ProductOrder> Orders => Set<ProductOrder>();
    public DbSet<ProductOrderItem> OrderItems => Set<ProductOrderItem>();
    public DbSet<ProductOrderPayment> OrderPayments => Set<ProductOrderPayment>();
    public DbSet<OrderFulfillment> OrderFulfillments => Set<OrderFulfillment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
