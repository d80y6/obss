using Obss.Provisioning.Infrastructure.Transports.Abstractions;

namespace Obss.Provisioning.Infrastructure.Transports.Snmp;

public enum SnmpVersion
{
    V1,
    V2C,
    V3
}

public sealed record SnmpTransportConfig : TransportConfigBase
{
    public SnmpVersion SnmpVersion { get; init; } = SnmpVersion.V2C;
    public string Community { get; init; } = "public";
    public string? V3UserName { get; init; }
    public string? V3AuthPassword { get; init; }
    public string? V3PrivPassword { get; init; }
    public string? V3ContextName { get; init; }
    public string? V3AuthProtocol { get; init; } = "SHA256";
    public string? V3PrivProtocol { get; init; } = "AES256";

    public override TransportProtocol Protocol => SnmpVersion switch
    {
        SnmpVersion.V1 => TransportProtocol.SnmpV1,
        SnmpVersion.V2C => TransportProtocol.SnmpV2C,
        SnmpVersion.V3 => TransportProtocol.SnmpV3,
        _ => TransportProtocol.SnmpV2C
    };
}
