using Microsoft.EntityFrameworkCore;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Orders.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository : EfRepository<Order>, IOrderRepository
{
    public OrderRepository(OrderDbContext context)
        : base(context)
    {
    }

    public async Task<Order?> GetByIdWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .Include(o => o.Fulfillment)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetFilteredAsync(
        Guid? customerId = null,
        OrderStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? orderType = null,
        string? searchTerm = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .Include(o => o.Fulfillment)
            .AsQueryable();

        if (customerId.HasValue)
        {
            query = query.Where(o => o.CustomerId == customerId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(o => o.OrderDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(o => o.OrderDate <= toDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(orderType) && Enum.TryParse<OrderType>(orderType, true, out var parsedType))
        {
            query = query.Where(o => o.OrderType == parsedType);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(o =>
                o.OrderNumber.Contains(searchTerm) ||
                o.CustomerName.Contains(searchTerm));
        }

        query = query
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByCustomerAsync(
        Guid customerId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.Items)
            .Include(o => o.Fulfillment)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(
        Guid? customerId = null,
        OrderStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (customerId.HasValue)
            query = query.Where(o => o.CustomerId == customerId.Value);

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        if (fromDate.HasValue)
            query = query.Where(o => o.OrderDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(o => o.OrderDate <= toDate.Value);

        return await query.CountAsync(cancellationToken);
    }
}
