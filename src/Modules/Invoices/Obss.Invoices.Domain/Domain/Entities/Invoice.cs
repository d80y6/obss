using Obss.Invoices.Domain.Events;
using Obss.Invoices.Domain.Exceptions;
using Obss.Invoices.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.Invoices.Domain.Entities;

public sealed record RelatedParty(string PartyId, string PartyName, string Role);

public class Invoice : AggregateRoot<Guid>
{
    private readonly List<InvoiceLine> _lines = [];
    private readonly List<InvoicePayment> _payments = [];
    private readonly List<InvoiceNote> _notes = [];
    private readonly List<RelatedParty> _relatedParties = [];

    private Invoice() { }

    private Invoice(
        Guid id,
        TenantId tenantId,
        string invoiceNumber,
        Guid customerId,
        string customerName,
        string customerEmail,
        string customerAddress,
        DateTime invoiceDate,
        DateTime dueDate,
        string currency)
        : base(id)
    {
        TenantId = tenantId;
        InvoiceNumber = invoiceNumber;
        CustomerId = customerId;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        CustomerAddress = customerAddress;
        InvoiceDate = invoiceDate;
        DueDate = dueDate;
        Status = InvoiceStatus.Draft;
        Currency = currency;
        SubTotal = 0;
        DiscountTotal = 0;
        TaxTotal = 0;
        GrandTotal = 0;
        AmountPaid = 0;
        AmountDue = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public TenantId TenantId { get; private set; } = default!;
    public string InvoiceNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerEmail { get; private set; } = string.Empty;
    public string CustomerAddress { get; private set; } = string.Empty;
    public DateTime InvoiceDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal DiscountTotal { get; private set; }
    public decimal TaxTotal { get; private set; }
    public decimal GrandTotal { get; private set; }
    public decimal AmountPaid { get; private set; }
    public decimal AmountDue { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public string? Href { get; private set; }
    public string? AtType { get; private set; } = "Invoice";
    public string? AtBaseType { get; private set; } = "CustomerBill";
#pragma warning disable S1144 // Used by EF Core via reflection
    public string? AtSchemaLocation { get; private set; }
#pragma warning restore S1144
#pragma warning disable S1144 // Used by EF Core via reflection
    public string? ExternalId { get; private set; }
#pragma warning restore S1144

    public IReadOnlyCollection<InvoiceLine> Lines => _lines.AsReadOnly();
    public IReadOnlyCollection<InvoicePayment> Payments => _payments.AsReadOnly();
    public IReadOnlyCollection<InvoiceNote> NotesCollection => _notes.AsReadOnly();
    public IReadOnlyCollection<RelatedParty> RelatedParties => _relatedParties.AsReadOnly();

    public static Invoice Create(
        TenantId tenantId,
        string invoiceNumber,
        Guid customerId,
        string customerName,
        string customerEmail,
        string customerAddress,
        DateTime invoiceDate,
        DateTime dueDate,
        string currency)
    {
        return new Invoice(
            Guid.NewGuid(),
            tenantId,
            invoiceNumber,
            customerId,
            customerName,
            customerEmail,
            customerAddress,
            invoiceDate,
            dueDate,
            currency);
    }

    public void AddLine(InvoiceLine line)
    {
        _lines.Add(line);
        CalculateAmounts();
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddLines(IEnumerable<InvoiceLine> lines)
    {
        foreach (var line in lines)
        {
            _lines.Add(line);
        }
        CalculateAmounts();
        UpdatedAt = DateTime.UtcNow;
    }

    public void CalculateAmounts()
    {
        SubTotal = _lines.Sum(l => l.LineType == LineType.Tax ? 0 : l.TotalAmount);
        DiscountTotal = _lines.Where(l => l.LineType == LineType.Discount).Sum(l => l.TotalAmount);
        TaxTotal = _lines.Sum(l => l.TaxAmount);
        GrandTotal = SubTotal - Math.Abs(DiscountTotal) + TaxTotal;
        AmountDue = GrandTotal - AmountPaid;
    }

    public void MarkAsFinalized()
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidInvoiceStateException($"Cannot finalize invoice in '{Status}' state. Only draft invoices can be finalized.");

        if (_lines.Count == 0)
            throw new InvalidInvoiceStateException("Cannot finalize an invoice with no lines.");

        Status = InvoiceStatus.Finalized;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new InvoiceFinalizedDomainEvent(Id, InvoiceNumber, CustomerId, GrandTotal));
    }

    public void MarkAsSent()
    {
        if (Status != InvoiceStatus.Finalized)
            throw new InvalidInvoiceStateException($"Cannot mark invoice as sent when in '{Status}' state. Invoice must be finalized first.");

        Status = InvoiceStatus.Sent;
        SentAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordPayment(decimal amount, string paymentRef)
    {
        if (Status != InvoiceStatus.Sent && Status != InvoiceStatus.Overdue)
            throw new InvalidInvoiceStateException($"Cannot record payment on invoice in '{Status}' state.");

        if (amount <= 0)
            throw new InvalidInvoiceStateException("Payment amount must be positive.");

        var remaining = GrandTotal - AmountPaid;
        if (amount > remaining)
            throw new InvalidInvoiceStateException($"Payment amount {amount} exceeds remaining balance {remaining}.");

        _payments.Add(new InvoicePayment(Guid.NewGuid(), Id, amount, paymentRef, DateTime.UtcNow));
        AmountPaid += amount;
        AmountDue = GrandTotal - AmountPaid;
        UpdatedAt = DateTime.UtcNow;

        if (AmountDue <= 0)
        {
            Status = InvoiceStatus.Paid;
            PaidAt = DateTime.UtcNow;
            AddDomainEvent(new InvoicePaidDomainEvent(Id, InvoiceNumber, amount, paymentRef));
        }
    }

    public void MarkAsOverdue()
    {
        if (Status != InvoiceStatus.Sent)
            throw new InvalidInvoiceStateException($"Cannot mark invoice as overdue when in '{Status}' state.");

        Status = InvoiceStatus.Overdue;
        UpdatedAt = DateTime.UtcNow;

        var daysOverdue = (int)(DateTime.UtcNow - DueDate).TotalDays;
        AddDomainEvent(new InvoiceOverdueDomainEvent(Id, InvoiceNumber, Math.Max(1, daysOverdue)));
    }

    public void Cancel(string reason)
    {
        if (Status == InvoiceStatus.Paid || Status == InvoiceStatus.Cancelled || Status == InvoiceStatus.Void)
            throw new InvalidInvoiceStateException($"Cannot cancel invoice in '{Status}' state.");

        Status = InvoiceStatus.Cancelled;
        Notes = reason;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new InvoiceCancelledDomainEvent(Id, InvoiceNumber, reason));
    }

    public void IssueCreditNote(decimal amount, string reason)
    {
        if (Status != InvoiceStatus.Sent && Status != InvoiceStatus.Paid && Status != InvoiceStatus.Overdue)
            throw new InvalidInvoiceStateException($"Cannot issue credit note on invoice in '{Status}' state.");

        if (amount <= 0 || amount > GrandTotal)
            throw new InvalidInvoiceStateException($"Invalid credit note amount {amount}. Must be between 0 and {GrandTotal}.");

        UpdatedAt = DateTime.UtcNow;
    }

    public void AddNote(string content)
    {
        _notes.Add(new InvoiceNote(Guid.NewGuid(), Id, content, DateTime.UtcNow));
        UpdatedAt = DateTime.UtcNow;
    }

    public void Void()
    {
        if (Status == InvoiceStatus.Void || Status == InvoiceStatus.Paid)
            throw new InvalidInvoiceStateException($"Cannot void invoice in '{Status}' state.");

        Status = InvoiceStatus.Void;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetHref(string href) => Href = href;

    public void AddRelatedParty(string partyId, string partyName, string role) => _relatedParties.Add(new RelatedParty(partyId, partyName, role));
}

public class InvoicePayment : Entity<Guid>
{
    private InvoicePayment() { }

    public InvoicePayment(Guid id, Guid invoiceId, decimal amount, string paymentReference, DateTime paidAt)
        : base(id)
    {
        InvoiceId = invoiceId;
        Amount = amount;
        PaymentReference = paymentReference;
        PaidAt = paidAt;
    }

    public Guid InvoiceId { get; private set; }
    public decimal Amount { get; private set; }
    public string PaymentReference { get; private set; } = string.Empty;
    public DateTime PaidAt { get; private set; }
}

public class InvoiceNote : Entity<Guid>
{
    private InvoiceNote() { }

    public InvoiceNote(Guid id, Guid invoiceId, string content, DateTime createdAt)
        : base(id)
    {
        InvoiceId = invoiceId;
        Content = content;
        CreatedAt = createdAt;
    }

    public Guid InvoiceId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
}

public sealed class InvoiceCancelledDomainEvent : DomainEvent, MediatR.INotification
{
    public InvoiceCancelledDomainEvent(Guid invoiceId, string invoiceNumber, string reason)
    {
        InvoiceId = invoiceId;
        InvoiceNumber = invoiceNumber;
        Reason = reason;
    }

    public Guid InvoiceId { get; }
    public string InvoiceNumber { get; }
    public string Reason { get; }
}
