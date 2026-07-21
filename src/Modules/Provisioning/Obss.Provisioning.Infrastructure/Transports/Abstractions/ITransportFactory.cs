namespace Obss.Provisioning.Infrastructure.Transports.Abstractions;

public interface ITransportFactory
{
    INetworkTransport CreateTransport(ITransportConfig config);
    bool SupportsProtocol(TransportProtocol protocol);
    IReadOnlyCollection<TransportProtocol> SupportedProtocols { get; }
}
