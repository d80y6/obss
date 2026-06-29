using Obss.ServiceInventory.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.ServiceInventory.Application.Abstractions;

public interface IServiceTopologyRepository : IRepository<ServiceTopology>
{
    Task<ServiceTopology?> GetByServiceIdWithLinksAsync(Guid serviceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TopologyLink>> GetUpstreamLinksAsync(Guid serviceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TopologyLink>> GetDownstreamLinksAsync(Guid serviceId, CancellationToken cancellationToken = default);
}
