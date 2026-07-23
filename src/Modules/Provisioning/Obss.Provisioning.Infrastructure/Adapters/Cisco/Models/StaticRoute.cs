namespace Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;

public sealed record StaticRoute(
    string Prefix,
    string NextHop,
    int? AdministrativeDistance,
    string? Interface);