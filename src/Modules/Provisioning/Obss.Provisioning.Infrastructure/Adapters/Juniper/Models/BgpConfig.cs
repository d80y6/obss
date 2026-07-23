namespace Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;

public sealed record BgpConfig(
    int AsNumber,
    string? RouterId,
    IReadOnlyList<BgpGroup>? Groups);

public sealed record BgpGroup(
    string Name,
    string? Type,
    int? PeerAs,
    string? LocalAddress,
    IReadOnlyList<BgpNeighbor>? Neighbors);

public sealed record BgpNeighbor(
    string RemoteAddress,
    int? RemoteAs,
    string? PeerGroup,
    string? Description);
