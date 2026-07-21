using Obss.Payments.Domain.Events;
using Obss.Payments.Domain.Exceptions;
using Obss.Payments.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Payments.Domain.Entities;

public sealed record RelatedParty(string PartyId, string PartyName, string Role);

public class Payment : AggregateRoot<Guid>, ITenantEntity
{
    private readonly List<PaymentAllocation> _allocations = [];
    private readonly List<Refund> _refunds = [];
    private readonly List<RelatedParty> _relatedParties = [];

    private Payment() { }

    private Payment(
        Guid id,
        string tenantId,
        string paymentNumber,
        Guid customerId,
        decimal amount,
        string currency,
        PaymentMethodType paymentMethod,
        string? paymentReference,
        Guid? invoiceId,
        string? notes)
        : base(id)
    {
        TenantId = tenantId;
        PaymentNumber = paymentNumber;
        CustomerId = customerId;
        InvoiceId = invoiceId;
        Amount = amount;
        Currency = currency;
        PaymentMethod = paymentMethod;
        PaymentReference = paymentReference ?? string.Empty;
        Status = PaymentStatus.Pending;
        Notes = notes;
        PaidAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string PaymentNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public Guid? InvoiceId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public PaymentMethodType PaymentMethod { get; private set; }
    public string PaymentReference { get; private set; } = string.Empty;
    public PaymentStatus Status { get; private set; }
    public DateTime PaidAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? Notes { get; private set; }
#pragma warning disable S1144 // Used by EF Core via reflection
    public string? Href { get; private set; }
    public string AtType { get; private set; } = "Payment";
    public string AtBaseType { get; private set; } = "BillPayment";
    public string? AtSchemaLocation { get; private set; }
    public string? ExternalId { get; private set; }
#pragma warning restore S1144
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<PaymentAllocation> Allocations => _allocations.AsReadOnly();
    public IReadOnlyCollection<Refund> Refunds => _refunds.AsReadOnly();
    public IReadOnlyCollection<RelatedParty> RelatedParties => _relatedParties.AsReadOnly();

    public void SetHref(string href) => Href = href;

    public void AddRelatedParty(string partyId, string partyName, string role) => _relatedParties.Add(new RelatedParty(partyId, partyName, role));

    public static Payment Create(
        string tenantId,
        string paymentNumber,
        Guid customerId,
        decimal amount,
        string currency,
        PaymentMethodType paymentMethod,
        string? paymentReference = null,
        Guid? invoiceId = null,
        string? notes = null)
    {
        return new Payment(
            Guid.NewGuid(),
            tenantId,
            paymentNumber,
            customerId,
            amount,
            currency,
            paymentMethod,
            paymentReference,
            invoiceId,
            notes);
    }

    public void Complete()
    {
        if (Status == PaymentStatus.Completed)
            throw new PaymentAlreadyCompletedException(Id);

        if (Status is PaymentStatus.Refunded or PaymentStatus.Failed or PaymentStatus.PartiallyRefunded)
            throw new InvalidPaymentStateException($"Cannot complete a payment in '{Status}' state.");

        Status = PaymentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        if (InvoiceId.HasValue)
        {
            _allocations.Add(new PaymentAllocation(Guid.NewGuid(), Id, InvoiceId.Value, Amount, DateTime.UtcNow));
        }

        AddDomainEvent(new PaymentCompletedDomainEvent(Id, PaymentNumber, InvoiceId, Amount, Currency));
    }

    public void Fail(string reason)
    {
        if (Status == PaymentStatus.Completed)
            throw new PaymentAlreadyCompletedException(Id);

        if (Status == PaymentStatus.Refunded)
            throw new InvalidPaymentStateException("Cannot fail a refunded payment.");

        Status = PaymentStatus.Failed;
        Notes = reason;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PaymentFailedDomainEvent(Id, reason));
    }

    public void Refund(decimal amount, string reason)
    {
        if (Status != PaymentStatus.Completed && Status != PaymentStatus.PartiallyRefunded)
            throw new InvalidPaymentStateException($"Cannot refund a payment in '{Status}' state.");

        var totalRefunded = _refunds.Sum(r => r.Amount);
        var availableForRefund = Amount - totalRefunded;

        if (amount <= 0 || amount > availableForRefund)
            throw new InsufficientPaymentAmountException(amount, availableForRefund);

        _refunds.Add(new Refund(Guid.NewGuid(), Id, amount, reason, DateTime.UtcNow));

        Status = totalRefunded + amount >= Amount
            ? PaymentStatus.Refunded
            : PaymentStatus.PartiallyRefunded;

        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PaymentRefundedDomainEvent(Id, amount, reason));
    }

    public void MarkAsPending()
    {
        if (Status == PaymentStatus.Completed)
            throw new PaymentAlreadyCompletedException(Id);

        Status = PaymentStatus.Pending;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AllocateToInvoice(Guid invoiceId, decimal amount)
    {
        if (Status != PaymentStatus.Completed)
            throw new InvalidPaymentStateException($"Cannot allocate payment in '{Status}' state.");

        var totalAllocated = _allocations.Sum(a => a.Amount);
        var available = Amount - totalAllocated;

        if (amount <= 0 || amount > available)
            throw new InsufficientPaymentAmountException(amount, available);

        _allocations.Add(new PaymentAllocation(Guid.NewGuid(), Id, invoiceId, amount, DateTime.UtcNow));
        UpdatedAt = DateTime.UtcNow;
    }
}
