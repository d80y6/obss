namespace Obss.NetworkInventory.Application.DTOs;

public sealed record NetworkElementDto(
    Guid Id,
    string TenantId,
    string Name,
    string Hostname,
    string IPAddress,
    string ElementType,
    string Vendor,
    string Model,
    string? SoftwareVersion,
    string? SerialNumber,
    string? Location,
    string Status,
    string? ManagementIP,
    bool IsManaged,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<NetworkInterfaceDto> Interfaces,
    List<NetworkIpAddressDto> IpAddresses);

public sealed record NetworkInterfaceDto(
    Guid Id,
    Guid NetworkElementId,
    string Name,
    string? Description,
    string InterfaceType,
    int Speed,
    string Status,
    string? MacAddress,
    int MTU,
    Guid? ConnectedToInterfaceId,
    DateTime CreatedAt);

public sealed record NetworkIpAddressDto(
    Guid Id,
    Guid NetworkElementId,
    Guid? NetworkInterfaceId,
    string IPAddress,
    string SubnetMask,
    string? Gateway,
    string AddressType,
    bool IsAvailable,
    bool IsReserved,
    string? AssignedTo);
