namespace Obss.Provisioning.Infrastructure.Adapters.Nokia.Models;

public sealed record InterfaceStatus(
    string PortId,
    string OperationalState,
    string AdministrativeState,
    long Speed
);