using Obss.NetworkInventory.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.NetworkInventory.Domain.Entities;

public class CapacityRecord : Entity<Guid>
{
    public CapacityRecord(
        Guid id,
        Guid elementId,
        Guid? interfaceId,
        CapacityType capacityType,
        decimal totalCapacity,
        decimal usedCapacity)
        : base(id)
    {
        ElementId = elementId;
        InterfaceId = interfaceId;
        CapacityType = capacityType;
        TotalCapacity = totalCapacity;
        UsedCapacity = usedCapacity;
        AvailableCapacity = totalCapacity - usedCapacity;
        UtilizationPercent = totalCapacity > 0 ? Math.Round(usedCapacity / totalCapacity * 100, 2) : 0;
        MeasuredAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }

    private CapacityRecord() { }

    public Guid ElementId { get; private set; }
    public Guid? InterfaceId { get; private set; }
    public CapacityType CapacityType { get; private set; }
    public decimal TotalCapacity { get; private set; }
    public decimal UsedCapacity { get; private set; }
    public decimal AvailableCapacity { get; private set; }
    public decimal UtilizationPercent { get; private set; }
    public DateTime MeasuredAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public void UpdateUsage(decimal totalCapacity, decimal usedCapacity)
    {
        TotalCapacity = totalCapacity;
        UsedCapacity = usedCapacity;
        AvailableCapacity = totalCapacity - usedCapacity;
        UtilizationPercent = totalCapacity > 0 ? Math.Round(usedCapacity / totalCapacity * 100, 2) : 0;
        MeasuredAt = DateTime.UtcNow;
    }

    public bool IsOverThreshold(decimal threshold)
    {
        return UtilizationPercent > threshold;
    }
}
