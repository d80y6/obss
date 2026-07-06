using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.NetworkInventory.Application.Abstractions;

public interface INetworkElementRepository : IRepository<NetworkElement>
{
    Task<NetworkElement?> GetByHostnameAsync(string hostname, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NetworkElement>> GetFilteredAsync(
        string? type,
        string? status,
        string? location,
        int offset,
        int limit,
        CancellationToken cancellationToken = default);
}
