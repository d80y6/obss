namespace Obss.Provisioning.Infrastructure.Transports.Abstractions;

public abstract record TransportConfigBase : ITransportConfig
{
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; }
    public int TimeoutSeconds { get; init; } = 30;
    public int MaxRetries { get; init; } = 3;
    public int RetryDelayMs { get; init; } = 1000;
    public abstract TransportProtocol Protocol { get; }
}
