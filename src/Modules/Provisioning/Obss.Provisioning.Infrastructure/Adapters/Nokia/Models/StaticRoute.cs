namespace Obss.Provisioning.Infrastructure.Adapters.Nokia.Models;

public sealed record StaticRoute(
    string Prefix,
    string NextHop,
    int? Preference,
    string? Tag
);