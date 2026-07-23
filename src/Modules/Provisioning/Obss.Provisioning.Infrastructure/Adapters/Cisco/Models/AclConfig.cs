namespace Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;

public sealed record AclEntry(
    int Sequence,
    string Action,
    string Source,
    string SourceWildcard,
    string? Destination,
    string? Log);

public sealed record AclConfig(
    string Name,
    IReadOnlyList<AclEntry> Entries);