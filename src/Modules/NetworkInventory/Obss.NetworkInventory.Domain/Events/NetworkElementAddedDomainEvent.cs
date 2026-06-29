using Obss.NetworkInventory.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.NetworkInventory.Domain.Events;

public sealed class NetworkElementAddedDomainEvent : DomainEvent
{
    public NetworkElementAddedDomainEvent(Guid networkElementId, TenantId tenantId, string name, ElementType elementType)
    {
        NetworkElementId = networkElementId;
        TenantId = tenantId;
        Name = name;
        ElementType = elementType;
    }

    public Guid NetworkElementId { get; }
    public TenantId TenantId { get; }
    public string Name { get; }
    public ElementType ElementType { get; }
}
