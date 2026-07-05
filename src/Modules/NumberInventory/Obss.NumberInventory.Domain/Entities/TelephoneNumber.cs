using Obss.NumberInventory.Domain.Events;
using Obss.NumberInventory.Domain.Exceptions;
using Obss.NumberInventory.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.NumberInventory.Domain.Entities;

public class TelephoneNumber : AggregateRoot<Guid>
{
    private TelephoneNumber() { }

    private TelephoneNumber(
        Guid id,
        string tenantId,
        string number,
        NumberType numberType,
        decimal cost,
        string currency,
        string? notes)
        : base(id)
    {
        TenantId = tenantId;
        Number = number;
        NumberType = numberType;
        Status = NumberStatus.Available;
        Cost = cost;
        Currency = currency;
        Notes = notes;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string Number { get; private set; } = string.Empty;
    public NumberType NumberType { get; private set; }
    public NumberStatus Status { get; private set; }
    public Guid? CustomerId { get; private set; }
    public Guid? SubscriptionId { get; private set; }
    public DateTime? AssignedAt { get; private set; }
    public DateTime? ReservedAt { get; private set; }
    public decimal Cost { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static TelephoneNumber Create(
        string tenantId,
        string number,
        NumberType numberType,
        decimal cost,
        string currency,
        string? notes = null)
    {
        return new TelephoneNumber(
            Guid.NewGuid(),
            tenantId,
            number,
            numberType,
            cost,
            currency,
            notes);
    }

    public void Assign(Guid customerId, Guid subscriptionId)
    {
        if (Status != NumberStatus.Available && Status != NumberStatus.Reserved)
            throw new InvalidNumberStateException(
                $"Cannot assign number in '{Status}' status. Only 'Available' or 'Reserved' numbers can be assigned.");

        Status = NumberStatus.Assigned;
        CustomerId = customerId;
        SubscriptionId = subscriptionId;
        AssignedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new NumberAssignedDomainEvent(
            Id, Number, customerId, subscriptionId));
    }

    public void Reserve()
    {
        if (Status != NumberStatus.Available)
            throw new InvalidNumberStateException(
                $"Cannot reserve number in '{Status}' status. Only 'Available' numbers can be reserved.");

        Status = NumberStatus.Reserved;
        ReservedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Release()
    {
        if (Status != NumberStatus.Reserved && Status != NumberStatus.Assigned)
            throw new InvalidNumberStateException(
                $"Cannot release number in '{Status}' status. Only 'Reserved' or 'Assigned' numbers can be released.");

        Status = NumberStatus.Available;
        CustomerId = null;
        SubscriptionId = null;
        AssignedAt = null;
        ReservedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Resume()
    {
        if (Status != NumberStatus.Suspended)
            throw new InvalidNumberStateException(
                $"Cannot resume number in '{Status}' status. Only 'Suspended' numbers can be resumed.");

        Status = NumberStatus.Assigned;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Suspend()
    {
        if (Status != NumberStatus.Assigned && Status != NumberStatus.Ported)
            throw new InvalidNumberStateException(
                $"Cannot suspend number in '{Status}' status. Only 'Assigned' or 'Ported' numbers can be suspended.");

        Status = NumberStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disconnect()
    {
        if (Status != NumberStatus.Assigned && Status != NumberStatus.Suspended && Status != NumberStatus.Ported)
            throw new InvalidNumberStateException(
                $"Cannot disconnect number in '{Status}' status.");

        Status = NumberStatus.Disconnected;
        CustomerId = null;
        SubscriptionId = null;
        AssignedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void PortIn(Guid? customerId = null)
    {
        if (Status != NumberStatus.Available)
            throw new InvalidNumberStateException(
                $"Cannot port in number in '{Status}' status. Only 'Available' numbers can be ported in.");

        Status = NumberStatus.Ported;
        CustomerId = customerId;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new NumberPortedDomainEvent(Id, Number, "In", customerId));
    }

    public void PortOut()
    {
        if (Status != NumberStatus.Assigned && Status != NumberStatus.Ported)
            throw new InvalidNumberStateException(
                $"Cannot port out number in '{Status}' status.");

        var previousCustomerId = CustomerId;
        Status = NumberStatus.Available;
        CustomerId = null;
        SubscriptionId = null;
        AssignedAt = null;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new NumberPortedDomainEvent(Id, Number, "Out", previousCustomerId));
    }
}
