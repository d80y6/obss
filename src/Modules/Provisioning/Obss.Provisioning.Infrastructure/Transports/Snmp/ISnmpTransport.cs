using Obss.Provisioning.Infrastructure.Transports.Abstractions;

namespace Obss.Provisioning.Infrastructure.Transports.Snmp;

public interface ISnmpTransport : INetworkTransport
{
    Task<TransportResult> GetAsync(string oid, CancellationToken cancellationToken = default);

    Task<TransportResult> GetBulkAsync(IEnumerable<string> oids, CancellationToken cancellationToken = default);

    Task<TransportResult> WalkAsync(string rootOid, CancellationToken cancellationToken = default);

    Task<TransportResult> SetAsync(string oid, string data, CancellationToken cancellationToken = default);

    Task<TransportResult> GetNextAsync(string oid, CancellationToken cancellationToken = default);
}
