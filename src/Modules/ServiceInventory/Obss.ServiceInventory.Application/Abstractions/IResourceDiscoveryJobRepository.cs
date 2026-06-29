using Obss.ServiceInventory.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.ServiceInventory.Application.Abstractions;

public interface IResourceDiscoveryJobRepository : IRepository<ResourceDiscoveryJob>
{
    Task<IReadOnlyList<ResourceDiscoveryJob>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ResourceDiscoveryJob>> GetUnmatchedAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
