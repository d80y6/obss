namespace Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;

public sealed record DeviceInventory(
    string Model,
    string SerialNumber,
    string PartNumber,
    string Description,
    string Version,
    IReadOnlyList<HardwareComponent> Components);

public sealed record HardwareComponent(
    string Name,
    string Model,
    string SerialNumber,
    string Description,
    string Version,
    string? PartNumber);
