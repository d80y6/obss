namespace Obss.NetworkInventory.Application.DTOs;

public sealed record PONPortDto(
    Guid Id,
    Guid OLTId,
    int PortNumber,
    string PortType,
    string Status,
    int MaxONT,
    int ConnectedONTCount,
    float MaxDistance);

public sealed record OLTDetailDto(
    Guid Id,
    string TenantId,
    string Name,
    string Hostname,
    string IPAddress,
    string Vendor,
    string Model,
    string? SoftwareVersion,
    string? Location,
    string Status,
    int MaxPONPorts,
    int UsedPONPorts,
    int MaxONTPerPort,
    int MaxBandwidth,
    DateTime CreatedAt,
    List<PONPortDto> PONPorts);
