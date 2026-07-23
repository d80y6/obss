namespace Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;

public sealed record InterfaceConfig(
    string Name,
    string Description,
    string? Unit,
    int? VlanId,
    string? IpAddress,
    int? PrefixLength,
    bool? AdminUp,
    int? Mtu);
