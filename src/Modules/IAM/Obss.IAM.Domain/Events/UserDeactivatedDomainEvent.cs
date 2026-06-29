using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.IAM.Domain.Events;

public sealed class UserDeactivatedDomainEvent : DomainEvent
{
    public UserDeactivatedDomainEvent(Guid userId, TenantId tenantId)
    {
        UserId = userId;
        TenantId = tenantId;
    }

    public Guid UserId { get; }
    public TenantId TenantId { get; }
}
