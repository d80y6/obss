namespace Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;

public sealed record AlarmInfo(
    string Id,
    string Severity,
    string Description,
    string? Source,
    DateTime? Timestamp);
