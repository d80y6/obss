namespace Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;

public sealed record SystemConfig(
    string? Hostname,
    string? DomainName,
    IReadOnlyList<string>? NtpServers,
    IReadOnlyList<string>? DnsServers,
    string? Username);