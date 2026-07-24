namespace Obss.Provisioning.Infrastructure.Adapters.Nokia.Models;

public sealed record SystemConfig(
    string? Name,
    string? DomainName,
    IReadOnlyList<string>? DnsServers,
    IReadOnlyList<string>? NtpServers,
    string? SnmpLocation
);