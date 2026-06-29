namespace Obss.NetworkInventory.Application.DTOs;

public sealed record CapacityRecordDto(
    Guid Id,
    Guid ElementId,
    Guid? InterfaceId,
    string CapacityType,
    decimal TotalCapacity,
    decimal UsedCapacity,
    decimal AvailableCapacity,
    decimal UtilizationPercent,
    DateTime MeasuredAt,
    DateTime CreatedAt);
