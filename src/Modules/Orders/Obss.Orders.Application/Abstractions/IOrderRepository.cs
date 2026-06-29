using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Orders.Application.Abstractions;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByIdWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetFilteredAsync(
        Guid? customerId = null,
        OrderStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? orderType = null,
        string? searchTerm = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByCustomerAsync(
        Guid customerId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(
        Guid? customerId = null,
        OrderStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);
}
