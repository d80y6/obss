using Obss.Provisioning.Infrastructure.Transports.Abstractions;

namespace Obss.Provisioning.Infrastructure.Transports.Netconf;

public sealed record NetconfTransportConfig : TransportConfigBase
{
    public string? Username { get; init; }
    public string? Password { get; init; }
    public string? PrivateKeyPath { get; init; }
    public int MaxMessageSize { get; init; } = 65535;
    public bool UseCandidateConfig { get; init; }

    public override TransportProtocol Protocol => TransportProtocol.Netconf;
}
