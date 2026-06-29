using Obss.SharedKernel.Domain.Common;

namespace Obss.NumberInventory.Domain.Events;

public sealed class NumberPortedDomainEvent : DomainEvent
{
    public NumberPortedDomainEvent(
        Guid numberId,
        string number,
        string direction,
        Guid? customerId)
    {
        NumberId = numberId;
        Number = number;
        Direction = direction;
        CustomerId = customerId;
    }

    public Guid NumberId { get; }
    public string Number { get; }
    public string Direction { get; }
    public Guid? CustomerId { get; }
}
