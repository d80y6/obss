namespace Obss.Provisioning.Infrastructure.Adapters.Nokia;

public sealed record NokiaAdapterConfig
{
    public string BaseUri { get; init; } = "http://localhost:8080";
    public bool UseSimulator { get; init; } = true;
    public string RestconfTransport { get; init; } = "nokia-restconf";
    public int TimeoutSeconds { get; init; } = 30;
}
