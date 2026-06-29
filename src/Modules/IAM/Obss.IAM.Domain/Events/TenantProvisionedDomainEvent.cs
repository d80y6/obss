using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.IAM.Domain.Events;

public sealed class TenantProvisionedDomainEvent : DomainEvent
{
    public TenantProvisionedDomainEvent(TenantId tenantId, string name, string slug)
    {
        TenantId = tenantId;
        Name = name;
        Slug = slug;
    }

    public TenantId TenantId { get; }
    public string Name { get; }
    public string Slug { get; }
}
