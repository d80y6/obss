using Obss.AAA.Domain.Entities;
using Obss.AAA.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.AAA.Application.Abstractions;

public interface INasRepository : IRepository<NetworkAccessServer>
{
    Task<NetworkAccessServer?> GetByIpAddressAsync(
        string ipAddress,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NetworkAccessServer>> GetByNasTypeAsync(
        NasType nasType,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NetworkAccessServer>> GetActiveNasDevicesAsync(
        CancellationToken cancellationToken = default);
}
