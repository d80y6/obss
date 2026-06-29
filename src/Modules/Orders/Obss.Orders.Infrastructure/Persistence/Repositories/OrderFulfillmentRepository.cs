using Microsoft.EntityFrameworkCore;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Orders.Infrastructure.Persistence.Repositories;

public sealed class OrderFulfillmentRepository : EfRepository<OrderFulfillment>, IOrderFulfillmentRepository
{
    public OrderFulfillmentRepository(OrderDbContext context)
        : base(context)
    {
    }

    public async Task<OrderFulfillment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(f => f.OrderId == orderId, cancellationToken);
    }

    public async Task<IReadOnlyList<OrderFulfillment>> GetByStatusAsync(FulfillmentStatus status, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(f => f.Status == status)
            .OrderBy(f => f.StartedAt)
            .ToListAsync(cancellationToken);
    }
}
