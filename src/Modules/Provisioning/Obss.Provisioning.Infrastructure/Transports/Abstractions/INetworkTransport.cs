namespace Obss.Provisioning.Infrastructure.Transports.Abstractions;

public interface INetworkTransport
{
    TransportProtocol Protocol { get; }
    ITransportConfig Config { get; }

    Task<TransportConnectionResult> TestConnectionAsync(CancellationToken cancellationToken = default);
}
