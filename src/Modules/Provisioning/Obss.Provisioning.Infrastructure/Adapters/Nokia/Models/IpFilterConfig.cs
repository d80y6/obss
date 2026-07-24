namespace Obss.Provisioning.Infrastructure.Adapters.Nokia.Models;

public sealed record IpFilterConfig(
    string Name,
    string? Description,
    IReadOnlyList<IpFilterEntry> Entries
);

public sealed record IpFilterEntry(
    int Sequence,
    string Action,
    string? SourceIp,
    string? DestinationIp,
    int? SourcePort,
    int? DestinationPort,
    string? Protocol,
    bool? Log
);