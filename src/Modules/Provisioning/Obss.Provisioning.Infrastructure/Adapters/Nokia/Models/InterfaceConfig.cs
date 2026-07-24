namespace Obss.Provisioning.Infrastructure.Adapters.Nokia.Models;

public sealed record InterfaceConfig(
    string PortId,
    string Description,
    bool? AdminUp,
    int? Mtu,
    string? VlanTag,
    string? IpAddress,
    int? PrefixLength
);