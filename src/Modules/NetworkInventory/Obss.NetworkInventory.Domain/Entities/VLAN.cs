using Obss.NetworkInventory.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.NetworkInventory.Domain.Entities;

public class VLAN : AggregateRoot<Guid>
{
    private VLAN() { }

    private VLAN(
        Guid id,
        TenantId tenantId,
        int vlanId,
        string name,
        string? description,
        string? location)
        : base(id)
    {
        TenantId = tenantId;
        VLANId = vlanId;
        Name = name;
        Description = description;
        Location = location;
        Status = VLANStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    public TenantId TenantId { get; private set; } = default!;
    public int VLANId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Location { get; private set; }
    public VLANStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static VLAN Create(
        TenantId tenantId,
        int vlanId,
        string name,
        string? description = null,
        string? location = null)
    {
        if (vlanId < 1 || vlanId > 4094)
            throw new ArgumentOutOfRangeException(nameof(vlanId), "VLAN ID must be between 1 and 4094.");

        return new VLAN(Guid.NewGuid(), tenantId, vlanId, name, description, location);
    }

    public void Reserve()
    {
        if (Status == VLANStatus.Reserved)
            return;
        Status = VLANStatus.Reserved;
    }

    public void Decommission()
    {
        Status = VLANStatus.Decommissioned;
    }
}
