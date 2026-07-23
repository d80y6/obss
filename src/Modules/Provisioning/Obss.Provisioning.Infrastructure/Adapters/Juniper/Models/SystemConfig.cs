namespace Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;

public sealed record SystemConfig(
    string? Hostname,
    string? DomainName,
    IReadOnlyList<string>? DnsServers,
    IReadOnlyList<string>? NtpServers,
    string? SyslogHost);
