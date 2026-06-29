using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.IAM.Domain.Events;

public sealed class UserCreatedDomainEvent : DomainEvent
{
    public UserCreatedDomainEvent(Guid userId, TenantId tenantId, Email email, string username)
    {
        UserId = userId;
        TenantId = tenantId;
        Email = email;
        Username = username;
    }

    public Guid UserId { get; }
    public TenantId TenantId { get; }
    public Email Email { get; }
    public string Username { get; }
}
