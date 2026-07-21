using Obss.Provisioning.Infrastructure.Transports.Abstractions;

namespace Obss.Provisioning.Infrastructure.Transports.Ssh;

public sealed record SshTransportConfig : TransportConfigBase
{
    public string? Username { get; init; }
    public string? Password { get; init; }
    public string? PrivateKeyPath { get; init; }
    public string? PrivateKeyPassphrase { get; init; }
    public bool UseSftp { get; init; } = true;
    public int ConnectionAttempts { get; init; } = 3;

    public override TransportProtocol Protocol => TransportProtocol.Ssh;
}
