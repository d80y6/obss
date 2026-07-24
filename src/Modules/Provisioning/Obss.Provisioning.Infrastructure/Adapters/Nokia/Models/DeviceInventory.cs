namespace Obss.Provisioning.Infrastructure.Adapters.Nokia.Models;

public sealed record DeviceInventory(
    string Model,
    string SerialNumber,
    string PartNumber,
    string Description,
    string Version,
    IReadOnlyList<ChassisComponent> Components
);

public sealed record ChassisComponent(
    string Name,
    string Type,
    string? SerialNumber,
    string? PartNumber,
    string? Description,
    string? Version
);