using Obss.Invoices.Domain.Events;
using Obss.Invoices.Domain.Exceptions;
using Obss.Invoices.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Invoices.Domain.Entities;

public class CreditNote : AggregateRoot<Guid>, ITenantEntity
{
    private readonly List<CreditNoteLine> _lines = [];

    private CreditNote() { }

    private CreditNote(
        Guid id,
        string tenantId,
        string creditNoteNumber,
        Guid invoiceId,
        Guid customerId,
        string reason,
        string currency)
        : base(id)
    {
        TenantId = tenantId;
        CreditNoteNumber = creditNoteNumber;
        InvoiceId = invoiceId;
        CustomerId = customerId;
        Reason = reason;
        Status = CreditNoteStatus.Draft;
        Currency = currency;
        SubTotal = 0;
        TaxAmount = 0;
        TotalAmount = 0;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string CreditNoteNumber { get; private set; } = string.Empty;
    public Guid InvoiceId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public CreditNoteStatus Status { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public DateTime IssuedAt { get; private set; }
    public DateTime? AppliedAt { get; private set; }
#pragma warning disable S1144 // Used by EF Core via reflection
    public string? ExternalId { get; private set; }
#pragma warning restore S1144

    public IReadOnlyCollection<CreditNoteLine> Lines => _lines.AsReadOnly();

    public static CreditNote Create(
        string tenantId,
        string creditNoteNumber,
        Guid invoiceId,
        Guid customerId,
        string reason,
        string currency)
    {
        return new CreditNote(
            Guid.NewGuid(),
            tenantId,
            creditNoteNumber,
            invoiceId,
            customerId,
            reason,
            currency);
    }

    public void AddLine(CreditNoteLine line)
    {
        _lines.Add(line);
        CalculateAmounts();
    }

    public void AddLines(IEnumerable<CreditNoteLine> lines)
    {
        foreach (var line in lines)
        {
            _lines.Add(line);
        }
        CalculateAmounts();
    }

    private void CalculateAmounts()
    {
        SubTotal = _lines.Sum(l => l.Amount);
        TaxAmount = 0;
        TotalAmount = SubTotal;
    }

    public void Issue()
    {
        if (Status != CreditNoteStatus.Draft)
            throw new InvalidInvoiceStateException($"Cannot issue credit note in '{Status}' state.");

        if (_lines.Count == 0)
            throw new InvalidInvoiceStateException("Cannot issue a credit note with no lines.");

        Status = CreditNoteStatus.Issued;
        IssuedAt = DateTime.UtcNow;

        AddDomainEvent(new CreditNoteIssuedDomainEvent(Id, InvoiceId, TotalAmount));
    }

    public void Apply()
    {
        if (Status != CreditNoteStatus.Issued)
            throw new InvalidInvoiceStateException($"Cannot apply credit note in '{Status}' state.");

        Status = CreditNoteStatus.Applied;
        AppliedAt = DateTime.UtcNow;
    }

    public void Void()
    {
        if (Status == CreditNoteStatus.Void)
            throw new InvalidInvoiceStateException("Credit note is already void.");

        Status = CreditNoteStatus.Void;
    }
}
