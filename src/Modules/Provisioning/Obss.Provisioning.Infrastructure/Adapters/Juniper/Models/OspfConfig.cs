namespace Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;

public sealed record OspfConfig(
    int ProcessId,
    string? RouterId,
    IReadOnlyList<OspfArea>? Areas);

public sealed record OspfArea(
    string AreaId,
    IReadOnlyList<string>? Interfaces);
