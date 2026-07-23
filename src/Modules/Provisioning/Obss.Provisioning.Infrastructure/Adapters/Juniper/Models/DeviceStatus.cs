namespace Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;

public sealed record DeviceStatus(
    string Hostname,
    string Version,
    string Model,
    string SerialNumber,
    long UptimeSeconds,
    long MemoryUsageBytes,
    double CpuUtilization,
    IReadOnlyList<InterfaceStatus> Interfaces);
