using Obss.Invoices.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Invoices.Domain.Entities;

public class InvoiceLine : Entity<Guid>
{
    private InvoiceLine() { }

    public InvoiceLine(
        Guid id,
        Guid invoiceId,
        Guid billId,
        Guid? billLineId,
        LineType lineType,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal totalAmount,
        decimal taxAmount,
        decimal taxRate,
        string currency)
        : base(id)
    {
        InvoiceId = invoiceId;
        BillId = billId;
        BillLineId = billLineId;
        LineType = lineType;
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalAmount = totalAmount;
        TaxAmount = taxAmount;
        TaxRate = taxRate;
        Currency = currency;
    }

    public Guid InvoiceId { get; private set; }
    public Guid BillId { get; private set; }
    public Guid? BillLineId { get; private set; }
    public LineType LineType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TaxRate { get; private set; }
    public string Currency { get; private set; } = string.Empty;
#pragma warning disable S1144 // Used by EF Core via reflection
    public string? ExternalId { get; private set; }
#pragma warning restore S1144

    public Invoice Invoice { get; private set; } = null!;
}
