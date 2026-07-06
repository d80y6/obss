using Obss.ServiceInventory.Domain.Entities;
using Obss.ServiceInventory.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.ServiceInventory.Application.Abstractions;

public interface IServiceRepository : IRepository<Service>
{
    Task<Service?> GetByIdWithResourcesAsync(Guid serviceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Service>> GetFilteredAsync(
        Guid? customerId,
        ServiceType? serviceType,
        ServiceStatus? status,
        int offset,
        int limit,
        CancellationToken cancellationToken = default);
    Task<int> CountFilteredAsync(
        Guid? customerId,
        ServiceType? serviceType,
        ServiceStatus? status,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Service>> GetBySubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
    Task<Service?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default);
}
