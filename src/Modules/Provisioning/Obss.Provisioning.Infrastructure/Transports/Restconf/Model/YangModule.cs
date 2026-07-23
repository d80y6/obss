namespace Obss.Provisioning.Infrastructure.Transports.Restconf.Model;

public sealed record YangModule(
    string Name,
    string? Revision,
    string Namespace,
    string? SchemaUri,
    IReadOnlyList<string> Features,
    IReadOnlyList<string> Deviations);