namespace Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;

public sealed record BgpNeighbor(string RemoteAddress, int RemoteAs, string? PeerGroup);

public sealed record BgpNetwork(string Prefix, int PrefixLength);

public sealed record BgpConfig(
    int AsNumber,
    string? RouterId,
    IReadOnlyList<BgpNeighbor>? Neighbors,
    IReadOnlyList<BgpNetwork>? Networks);