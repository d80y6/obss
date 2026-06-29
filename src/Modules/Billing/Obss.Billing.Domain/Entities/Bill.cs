using Obss.Billing.Domain.Events;
using Obss.Billing.Domain.Exceptions;
using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Billing.Domain.Entities;

public class Bill : AggregateRoot<Guid>
{
    private readonly List<BillLine> _lines = [];

    private Bill() { }

    private Bill(
        Guid id,
        string tenantId,
        Guid customerId,
        string customerName,
        BillingPeriod billingPeriod,
        DateTime billingPeriodStart,
        DateTime billingPeriodEnd,
        DateTime dueDate,
        string currency)
        : base(id)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        CustomerName = customerName;
        BillingPeriod = billingPeriod;
        BillingPeriodStart = billingPeriodStart;
        BillingPeriodEnd = billingPeriodEnd;
        DueDate = dueDate;
        Currency = currency;
        Status = BillStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public BillingPeriod BillingPeriod { get; private set; }
    public DateTime BillingPeriodStart { get; private set; }
    public DateTime BillingPeriodEnd { get; private set; }
    public DateTime DueDate { get; private set; }
    public BillStatus Status { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal DiscountTotal { get; private set; }
    public decimal TaxTotal { get; private set; }
    public decimal GrandTotal { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? FinalizedAt { get; private set; }

    public IReadOnlyCollection<BillLine> Lines => _lines.AsReadOnly();

    public static Bill Create(
        string tenantId,
        Guid customerId,
        string customerName,
        BillingPeriod billingPeriod,
        DateTime billingPeriodStart,
        DateTime billingPeriodEnd,
        DateTime dueDate,
        string currency)
    {
        return new Bill(
            Guid.NewGuid(),
            tenantId,
            customerId,
            customerName,
            billingPeriod,
            billingPeriodStart,
            billingPeriodEnd,
            dueDate,
            currency);
    }

    public void AddLine(BillLine line)
    {
        if (Status is BillStatus.Finalized or BillStatus.Invoiced or BillStatus.Closed)
            throw new InvalidBillStateException("Cannot add lines to a finalized bill.");

        _lines.Add(line);
    }

    public void AddTaxLine(BillLine taxLine)
    {
        if (Status is BillStatus.Finalized or BillStatus.Invoiced or BillStatus.Closed)
            throw new InvalidBillStateException("Cannot add tax lines to a finalized bill.");

        _lines.Add(taxLine);
    }

    public void RecalculateTotalsWithTax()
    {
        if (Status is BillStatus.Finalized or BillStatus.Invoiced or BillStatus.Closed)
            throw new InvalidBillStateException("Cannot recalculate totals for a finalized bill.");

        SubTotal = 0;
        DiscountTotal = 0;
        TaxTotal = 0;

        foreach (var line in _lines.Where(l => l.LineType != LineType.Tax))
        {
            line.CalculateTotal();
            SubTotal += line.UnitPrice * line.Quantity;
            DiscountTotal += line.DiscountAmount;
        }

        foreach (var line in _lines.Where(l => l.LineType == LineType.Tax))
        {
            TaxTotal += line.UnitPrice;
        }

        GrandTotal = SubTotal - DiscountTotal + TaxTotal;
    }

    public void CalculateTotals()
    {
        if (Status != BillStatus.Draft)
            throw new InvalidBillStateException("Cannot calculate totals for a bill that is not in Draft status.");

        SubTotal = 0;
        DiscountTotal = 0;
        TaxTotal = 0;

        foreach (var line in _lines)
        {
            line.CalculateTotal();
            SubTotal += line.UnitPrice * line.Quantity;
            DiscountTotal += line.DiscountAmount;
            TaxTotal += line.TaxAmount;
        }

        GrandTotal = SubTotal - DiscountTotal + TaxTotal;
        Status = BillStatus.Calculated;

        AddDomainEvent(new BillCalculatedDomainEvent(Id, CustomerId, GrandTotal, Currency));
    }

    public void MarkAsFinalized()
    {
        if (Status != BillStatus.Calculated)
            throw new InvalidBillStateException("Cannot finalize a bill that is not in Calculated status.");

        Status = BillStatus.Finalized;
        FinalizedAt = DateTime.UtcNow;

        AddDomainEvent(new BillFinalizedDomainEvent(Id, CustomerId, GrandTotal, Currency));
    }

    public void MarkAsInvoiced()
    {
        if (Status != BillStatus.Finalized)
            throw new InvalidBillStateException("Cannot mark as invoiced a bill that is not in Finalized status.");

        Status = BillStatus.Invoiced;
    }

    public void Close()
    {
        if (Status != BillStatus.Invoiced)
            throw new InvalidBillStateException("Cannot close a bill that is not in Invoiced status.");

        Status = BillStatus.Closed;
    }
}
