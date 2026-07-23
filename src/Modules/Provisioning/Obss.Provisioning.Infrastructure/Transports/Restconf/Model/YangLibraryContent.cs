namespace Obss.Provisioning.Infrastructure.Transports.Restconf.Model;

public sealed record YangLibraryContent(
    string ContentId,
    IReadOnlyList<YangModule> Modules,
    DateTime FetchedAt);
