using Obss.SharedKernel.Domain.Common;

namespace Obss.Billing.Domain.Events;

public sealed class TaxAppliedDomainEvent : DomainEvent
{
    public TaxAppliedDomainEvent(Guid billId, Guid taxRuleId, string taxName, decimal taxRate, decimal taxAmount, string currency)
    {
        BillId = billId;
        TaxRuleId = taxRuleId;
        TaxName = taxName;
        TaxRate = taxRate;
        TaxAmount = taxAmount;
        Currency = currency;
    }

    public Guid BillId { get; }
    public Guid TaxRuleId { get; }
    public string TaxName { get; }
    public decimal TaxRate { get; }
    public decimal TaxAmount { get; }
    public string Currency { get; }
}
