using Obss.Collections.Domain.Events;
using Obss.Collections.Domain.Exceptions;
using Obss.Collections.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Collections.Domain.Entities;

public class CollectionCase : AggregateRoot<Guid>, ITenantEntity
{
    private readonly List<CollectionAction> _actions = [];
    private readonly List<PaymentArrangement> _paymentArrangements = [];

    private CollectionCase() { }

    private CollectionCase(
        Guid id,
        string tenantId,
        Guid customerId,
        string customerName,
        decimal totalOverdueAmount,
        string currency)
        : base(id)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        CustomerName = customerName;
        Status = CollectionCaseStatus.Open;
        TotalOverdueAmount = totalOverdueAmount;
        Currency = currency;
        CurrentDunningLevel = 0;
        OpenedAt = DateTime.UtcNow;
        LastActionAt = null;
        ResolvedAt = null;
        AssignedTo = null;
    }

    public string TenantId { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public CollectionCaseStatus Status { get; private set; }
    public decimal TotalOverdueAmount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public int CurrentDunningLevel { get; private set; }
    public DateTime OpenedAt { get; private set; }
    public DateTime? LastActionAt { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public string? AssignedTo { get; private set; }
    public string? Notes { get; private set; }

    public IReadOnlyCollection<CollectionAction> Actions => _actions.AsReadOnly();
    public IReadOnlyCollection<PaymentArrangement> PaymentArrangements => _paymentArrangements.AsReadOnly();

    public static CollectionCase Open(
        string tenantId,
        Guid customerId,
        string customerName,
        decimal totalOverdueAmount,
        string currency)
    {
        var @case = new CollectionCase(
            Guid.NewGuid(),
            tenantId,
            customerId,
            customerName,
            totalOverdueAmount,
            currency);

        @case.AddDomainEvent(new CollectionCaseOpenedDomainEvent(
            @case.Id, customerId, customerName, totalOverdueAmount, currency));

        return @case;
    }

    public void AssignTo(string userId)
    {
        if (Status == CollectionCaseStatus.Closed || Status == CollectionCaseStatus.Resolved)
            throw new InvalidCollectionStateException("Cannot assign a resolved or closed case.");

        AssignedTo = userId;
        LastActionAt = DateTime.UtcNow;
    }

    public void AddAction(CollectionAction action)
    {
        _actions.Add(action);
        LastActionAt = DateTime.UtcNow;

        if (action.ActionType == CollectionActionType.Escalation)
        {
            CurrentDunningLevel++;
        }

        if (Status == CollectionCaseStatus.Open)
        {
            Status = CollectionCaseStatus.InProgress;
        }
    }

    public PaymentArrangement CreatePaymentArrangement(
        decimal totalAmount,
        int installmentCount,
        decimal installmentAmount,
        PaymentFrequency frequency,
        DateTime firstPaymentDate)
    {
        if (Status == CollectionCaseStatus.Closed || Status == CollectionCaseStatus.Resolved)
            throw new InvalidCollectionStateException("Cannot create arrangement on a resolved or closed case.");

        if (_paymentArrangements.Any(pa => pa.Status == ArrangementStatus.Active || pa.Status == ArrangementStatus.Proposed))
            throw new DuplicatePaymentArrangementException(Id);

        var arrangement = PaymentArrangement.Create(
            Id, CustomerId, totalAmount, installmentCount,
            installmentAmount, frequency, firstPaymentDate);

        _paymentArrangements.Add(arrangement);

        AddDomainEvent(new PaymentArrangementCreatedDomainEvent(
            arrangement.Id, Id, CustomerId, totalAmount, installmentCount));

        return arrangement;
    }

    public void UpdateOverdueAmount(decimal amount)
    {
        TotalOverdueAmount = amount;
        LastActionAt = DateTime.UtcNow;
    }

    public void Resolve()
    {
        if (Status == CollectionCaseStatus.Closed || Status == CollectionCaseStatus.Resolved)
            throw new InvalidCollectionStateException("Case is already resolved or closed.");

        Status = CollectionCaseStatus.Resolved;
        ResolvedAt = DateTime.UtcNow;
        LastActionAt = DateTime.UtcNow;

        AddDomainEvent(new CollectionCaseResolvedDomainEvent(
            Id, CustomerId, TotalOverdueAmount, Currency));
    }

    public void Close()
    {
        if (Status != CollectionCaseStatus.Resolved)
            throw new InvalidCollectionStateException("Only resolved cases can be closed.");

        Status = CollectionCaseStatus.Closed;
        LastActionAt = DateTime.UtcNow;
    }

    public void AddNote(string note)
    {
        Notes = string.IsNullOrEmpty(Notes) ? note : $"{Notes}\n{note}";
        LastActionAt = DateTime.UtcNow;
    }

    public void AdvanceDunningLevel()
    {
        CurrentDunningLevel++;
        LastActionAt = DateTime.UtcNow;
    }
}
