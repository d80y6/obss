namespace Obss.Provisioning.Infrastructure.Transports.Abstractions;

public interface ITransportConfig
{
    string Host { get; }
    int Port { get; }
    int TimeoutSeconds { get; }
    int MaxRetries { get; }
    int RetryDelayMs { get; }
    TransportProtocol Protocol { get; }
}
