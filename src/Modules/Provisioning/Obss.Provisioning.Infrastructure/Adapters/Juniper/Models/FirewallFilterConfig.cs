namespace Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;

public sealed record FirewallFilterConfig(
    string Name,
    IReadOnlyList<FirewallFilterTerm> Terms);

public sealed record FirewallFilterTerm(
    string Name,
    string Action,
    string? SourceAddress,
    string? DestinationAddress,
    int? SourcePort,
    int? DestinationPort,
    string? Protocol,
    bool? Log);
