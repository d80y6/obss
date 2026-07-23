namespace Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;

public sealed record InterfaceStatus(
    string Name,
    string OperationalStatus,
    string AdministrativeStatus,
    long Speed);
