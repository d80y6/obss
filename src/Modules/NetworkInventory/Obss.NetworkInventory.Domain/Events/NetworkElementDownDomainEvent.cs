using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.NetworkInventory.Domain.Events;

public sealed class NetworkElementDownDomainEvent : DomainEvent
{
    public NetworkElementDownDomainEvent(Guid networkElementId, TenantId tenantId, string name, string hostname, string reason)
    {
        NetworkElementId = networkElementId;
        TenantId = tenantId;
        Name = name;
        Hostname = hostname;
        Reason = reason;
    }

    public Guid NetworkElementId { get; }
    public TenantId TenantId { get; }
    public string Name { get; }
    public string Hostname { get; }
    public string Reason { get; }
}
