using Obss.Provisioning.Infrastructure.Transports.Abstractions;

namespace Obss.Provisioning.Infrastructure.Transports.Netconf;

public interface INetconfTransport : INetworkTransport
{
    Task<TransportResult> GetConfigAsync(string source = "running", CancellationToken cancellationToken = default);

    Task<TransportResult> EditConfigAsync(string xmlConfig, string target = "running", CancellationToken cancellationToken = default);

    Task<TransportResult> CopyConfigAsync(string source, string target, CancellationToken cancellationToken = default);

    Task<TransportResult> ExecuteRpcAsync(string xmlRpc, CancellationToken cancellationToken = default);

    Task<TransportResult> LockAsync(string target = "running", CancellationToken cancellationToken = default);

    Task<TransportResult> UnlockAsync(string target = "running", CancellationToken cancellationToken = default);

    Task<TransportResult> GetSchemaAsync(string moduleName, CancellationToken cancellationToken = default);
}
