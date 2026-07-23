namespace Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;

public sealed record AlarmInfo(
    string Timestamp,
    string Severity,
    string Description,
    string? Source);