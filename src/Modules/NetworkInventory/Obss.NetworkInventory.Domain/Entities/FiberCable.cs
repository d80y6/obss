using Obss.NetworkInventory.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.NetworkInventory.Domain.Entities;

public class FiberCable : Entity<Guid>
{
    public FiberCable(
        Guid id,
        Guid fromElementId,
        Guid fromInterfaceId,
        Guid toElementId,
        Guid toInterfaceId,
        int length,
        int fiberCount,
        FiberType fiberType,
        int? splicingPoints,
        string? notes)
        : base(id)
    {
        FromElementId = fromElementId;
        FromInterfaceId = fromInterfaceId;
        ToElementId = toElementId;
        ToInterfaceId = toInterfaceId;
        Length = length;
        FiberCount = fiberCount;
        FiberType = fiberType;
        Status = FiberStatus.Active;
        SplicingPoints = splicingPoints;
        Notes = notes;
    }

    private FiberCable() { }

    public Guid FromElementId { get; private set; }
    public Guid FromInterfaceId { get; private set; }
    public Guid ToElementId { get; private set; }
    public Guid ToInterfaceId { get; private set; }
    public int Length { get; private set; }
    public int FiberCount { get; private set; }
    public FiberType FiberType { get; private set; }
    public FiberStatus Status { get; private set; }
    public int? SplicingPoints { get; private set; }
    public string? Notes { get; private set; }

    public void MarkDamaged()
    {
        Status = FiberStatus.Damaged;
    }

    public void Decommission()
    {
        Status = FiberStatus.Decommissioned;
    }

    public void Repair()
    {
        Status = FiberStatus.Active;
    }
}
