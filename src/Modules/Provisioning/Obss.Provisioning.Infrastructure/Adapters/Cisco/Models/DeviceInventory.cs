namespace Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;

public sealed record HardwareComponent(string Name, string PartNumber, string Status);

public sealed record DeviceInventory(
    string Model,
    string SerialNumber,
    string SoftwareVersion,
    string Memory,
    string Storage,
    IReadOnlyList<HardwareComponent> Components);