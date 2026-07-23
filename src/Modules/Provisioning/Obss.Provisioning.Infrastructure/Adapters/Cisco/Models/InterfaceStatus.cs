namespace Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;

public sealed record InterfaceStatus(
    string InterfaceName,
    string AdminStatus,
    string OperStatus,
    long Speed);