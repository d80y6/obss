using Obss.NetworkInventory.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.NetworkInventory.Domain.Entities;

public class ConnectivityLink : AggregateRoot<Guid>
{
    private ConnectivityLink() { }

    private ConnectivityLink(
        Guid id,
        TenantId tenantId,
        string name,
        string? description,
        Guid sourceElementId,
        Guid sourceInterfaceId,
        Guid targetElementId,
        Guid targetInterfaceId,
        LinkType linkType,
        int bandwidth,
        string? protocol,
        int latencyMs,
        int mtu)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        SourceElementId = sourceElementId;
        SourceInterfaceId = sourceInterfaceId;
        TargetElementId = targetElementId;
        TargetInterfaceId = targetInterfaceId;
        LinkType = linkType;
        Bandwidth = bandwidth;
        Status = LinkStatus.Active;
        Protocol = protocol;
        LatencyMs = latencyMs;
        MTU = mtu;
        CreatedAt = DateTime.UtcNow;
    }

    public TenantId TenantId { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid SourceElementId { get; private set; }
    public Guid SourceInterfaceId { get; private set; }
    public Guid TargetElementId { get; private set; }
    public Guid TargetInterfaceId { get; private set; }
    public LinkType LinkType { get; private set; }
    public int Bandwidth { get; private set; }
    public LinkStatus Status { get; private set; }
    public string? Protocol { get; private set; }
    public int LatencyMs { get; private set; }
    public int MTU { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static ConnectivityLink Create(
        TenantId tenantId,
        string name,
        string? description,
        Guid sourceElementId,
        Guid sourceInterfaceId,
        Guid targetElementId,
        Guid targetInterfaceId,
        LinkType linkType,
        int bandwidth,
        string? protocol,
        int latencyMs,
        int mtu)
    {
        return new ConnectivityLink(
            Guid.NewGuid(),
            tenantId,
            name,
            description,
            sourceElementId,
            sourceInterfaceId,
            targetElementId,
            targetInterfaceId,
            linkType,
            bandwidth,
            protocol,
            latencyMs,
            mtu);
    }

    public void BringUp()
    {
        if (Status == LinkStatus.Active)
            return;

        Status = LinkStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void TakeDown(string reason)
    {
        if (Status == LinkStatus.Down)
            return;

        Status = LinkStatus.Down;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetMaintenance()
    {
        if (Status == LinkStatus.Maintenance)
            return;

        Status = LinkStatus.Maintenance;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Resume()
    {
        BringUp();
    }

    public void UpdateBandwidth(int bandwidth)
    {
        Bandwidth = bandwidth;
        UpdatedAt = DateTime.UtcNow;
    }
}
