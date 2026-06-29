using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Orders.Application.Abstractions;

public interface IOrderFulfillmentRepository : IRepository<OrderFulfillment>
{
    Task<OrderFulfillment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderFulfillment>> GetByStatusAsync(FulfillmentStatus status, CancellationToken cancellationToken = default);
}
