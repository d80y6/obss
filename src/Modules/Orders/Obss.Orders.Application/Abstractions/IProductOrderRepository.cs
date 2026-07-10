using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Orders.Application.Abstractions;

public interface IProductOrderRepository : IRepository<ProductOrder>
{
    Task<ProductOrder?> GetByIdWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductOrder>> GetFilteredAsync(
        Guid? customerId = null,
        OrderStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? orderType = null,
        string? searchTerm = null,
        int offset = 0,
        int limit = 20,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductOrder>> GetByCustomerAsync(
        Guid customerId,
        int offset = 0,
        int limit = 20,
        CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(
        Guid? customerId = null,
        OrderStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);
}
