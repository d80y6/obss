using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.CRM.Domain.Events;

public sealed class CustomerCreatedDomainEvent : DomainEvent
{
    public CustomerCreatedDomainEvent(
        string userId,
        string tenantId,
        Guid customerId,
        CustomerType customerType,
        string displayName)
    {
        UserId = userId;
        TenantId = tenantId;
        CustomerId = customerId;
        CustomerType = customerType;
        DisplayName = displayName;
    }

    public string UserId { get; }
    public string TenantId { get; }
    public Guid CustomerId { get; }
    public CustomerType CustomerType { get; }
    public string DisplayName { get; }
}
