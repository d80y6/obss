using Obss.Provisioning.Infrastructure.Transports.Abstractions;

namespace Obss.Provisioning.Infrastructure.Transports.Restconf;

public sealed record RestconfTransportConfig : TransportConfigBase
{
    public override TransportProtocol Protocol => TransportProtocol.Restconf;
    public string BaseUri { get; set; } = string.Empty;
    public int YangLibraryCacheTtlSeconds { get; set; } = 300;
}
