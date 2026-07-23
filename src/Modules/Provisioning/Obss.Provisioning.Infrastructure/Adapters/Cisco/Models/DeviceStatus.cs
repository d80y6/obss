namespace Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;

public sealed record DeviceStatus(
    string Hostname,
    string SoftwareVersion,
    string Model,
    string Uptime,
    double CpuUtilization,
    double MemoryUtilization,
    IReadOnlyList<InterfaceStatus> Interfaces);