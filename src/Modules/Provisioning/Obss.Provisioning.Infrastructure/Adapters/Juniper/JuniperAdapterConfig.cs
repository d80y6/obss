namespace Obss.Provisioning.Infrastructure.Adapters.Juniper;

public sealed record JuniperAdapterConfig
{
    public string BaseUri { get; init; } = "http://localhost:8080";
    public bool UseSimulator { get; init; } = true;
    public string RestconfTransport { get; init; } = "juniper-restconf";
    public int TimeoutSeconds { get; init; } = 30;
}
