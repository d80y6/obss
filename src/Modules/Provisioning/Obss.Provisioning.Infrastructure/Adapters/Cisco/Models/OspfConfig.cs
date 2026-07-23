namespace Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;

public sealed record OspfArea(int AreaId, IReadOnlyList<string>? Interfaces);

public sealed record OspfConfig(
    int ProcessId,
    string? RouterId,
    IReadOnlyList<OspfArea>? Areas);