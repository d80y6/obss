namespace Obss.Provisioning.Infrastructure.Adapters.ZTE;

public sealed record ZteAdapterConfig
{
    public string? SoftswitchHost { get; init; }
    public int Port { get; init; } = ZteAdapterConstants.DefaultPort;
    public string? Protocol { get; init; } = ZteAdapterConstants.DefaultProtocol;
    public string? Ss7PointCode { get; init; }
    public bool UseSsl { get; init; }
    public int TimeoutSeconds { get; init; } = ZteAdapterConstants.DefaultTimeoutSeconds;
    public int MaxRetries { get; init; } = ZteAdapterConstants.DefaultMaxRetries;
    public string? OperationProfileVersion { get; init; } = ZteAdapterConstants.DefaultProfileVersion;
    public string? VendorProductName { get; init; }
    public bool EnableSimulator { get; init; } = true;
}
