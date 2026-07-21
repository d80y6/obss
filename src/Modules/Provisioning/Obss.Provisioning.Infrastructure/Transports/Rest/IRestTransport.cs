using Obss.Provisioning.Infrastructure.Transports.Abstractions;

namespace Obss.Provisioning.Infrastructure.Transports.Rest;

public interface IRestTransport : INetworkTransport
{
    Task<TransportResult> GetAsync(string endpoint, CancellationToken cancellationToken = default);

    Task<TransportResult> PostAsync(string endpoint, object? body = null, CancellationToken cancellationToken = default);

    Task<TransportResult> PutAsync(string endpoint, object? body = null, CancellationToken cancellationToken = default);

    Task<TransportResult> PatchAsync(string endpoint, object? body = null, CancellationToken cancellationToken = default);

    Task<TransportResult> DeleteAsync(string endpoint, CancellationToken cancellationToken = default);
}
