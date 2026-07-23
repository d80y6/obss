namespace Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;

public sealed record StaticRoute(
    string Prefix,
    string NextHop,
    int? Preference,
    string? QualifiedNextHop,
    string? Tag);
