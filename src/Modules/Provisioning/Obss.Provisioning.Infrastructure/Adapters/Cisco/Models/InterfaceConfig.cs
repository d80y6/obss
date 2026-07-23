namespace Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;

public sealed record InterfaceConfig(
    string Name,
    string Type,
    string? Description,
    string? IpAddress,
    int? PrefixLength,
    bool? AdminUp,
    int? Mtu,
    string? VlanId,
    IReadOnlyList<string>? SwitchportModes);