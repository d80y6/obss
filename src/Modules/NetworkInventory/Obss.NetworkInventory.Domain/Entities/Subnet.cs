using Obss.NetworkInventory.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.NetworkInventory.Domain.Entities;

public class Subnet : AggregateRoot<Guid>
{
    private Subnet() { }

    private Subnet(
        Guid id,
        TenantId tenantId,
        string network,
        string name,
        string? description,
        string? gateway,
        int vlanId,
        string? location)
        : base(id)
    {
        TenantId = tenantId;
        Network = network;
        Name = name;
        Description = description;
        Gateway = gateway;
        VLANId = vlanId;
        Location = location;
        Status = SubnetStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    public TenantId TenantId { get; private set; } = default!;
    public string Network { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Gateway { get; private set; }
    public int VLANId { get; private set; }
    public string? Location { get; private set; }
    public SubnetStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Subnet Create(
        TenantId tenantId,
        string network,
        string name,
        string? description = null,
        string? gateway = null,
        int vlanId = 0,
        string? location = null)
    {
        return new Subnet(Guid.NewGuid(), tenantId, network, name, description, gateway, vlanId, location);
    }

    public void Update(string name, string? description, string? gateway, int vlanId, string? location)
    {
        Name = name;
        Description = description;
        Gateway = gateway;
        VLANId = vlanId;
        Location = location;
    }

    public void Reserve()
    {
        if (Status == SubnetStatus.Reserved)
            return;
        Status = SubnetStatus.Reserved;
    }

    public void MarkExhausted()
    {
        Status = SubnetStatus.Exhausted;
    }

    public void Decommission()
    {
        Status = SubnetStatus.Decommissioned;
    }
}
