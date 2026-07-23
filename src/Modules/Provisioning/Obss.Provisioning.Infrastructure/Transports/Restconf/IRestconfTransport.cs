using Obss.Provisioning.Infrastructure.Transports.Abstractions;
using Obss.Provisioning.Infrastructure.Transports.Restconf.Model;

namespace Obss.Provisioning.Infrastructure.Transports.Restconf;

public interface IRestconfTransport : INetworkTransport
{
    Task<RestconfResult> GetAsync(string path, RestconfQueryParams? query = null, CancellationToken ct = default);
    Task<RestconfResult> PostAsync(string path, object? body = null, CancellationToken ct = default);
    Task<RestconfResult> PutAsync(string path, object? body = null, CancellationToken ct = default);
    Task<RestconfResult> PatchAsync(string path, object? body = null, CancellationToken ct = default);
    Task<RestconfResult> DeleteAsync(string path, CancellationToken ct = default);
    Task<YangLibraryContent> GetYangLibraryAsync(CancellationToken ct = default);
}
