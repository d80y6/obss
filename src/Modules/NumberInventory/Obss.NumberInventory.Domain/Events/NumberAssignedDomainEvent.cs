using Obss.SharedKernel.Domain.Common;

namespace Obss.NumberInventory.Domain.Events;

public sealed class NumberAssignedDomainEvent : DomainEvent
{
    public NumberAssignedDomainEvent(
        Guid numberId,
        string number,
        Guid customerId,
        Guid subscriptionId)
    {
        NumberId = numberId;
        Number = number;
        CustomerId = customerId;
        SubscriptionId = subscriptionId;
    }

    public Guid NumberId { get; }
    public string Number { get; }
    public Guid CustomerId { get; }
    public Guid SubscriptionId { get; }
}
