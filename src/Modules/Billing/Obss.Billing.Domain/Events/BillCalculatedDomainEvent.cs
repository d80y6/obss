using Obss.SharedKernel.Domain.Common;

namespace Obss.Billing.Domain.Events;

public sealed class BillCalculatedDomainEvent : DomainEvent
{
    public BillCalculatedDomainEvent(Guid billId, Guid customerId, decimal grandTotal, string currency)
    {
        BillId = billId;
        CustomerId = customerId;
        GrandTotal = grandTotal;
        Currency = currency;
    }

    public Guid BillId { get; }
    public Guid CustomerId { get; }
    public decimal GrandTotal { get; }
    public string Currency { get; }
}
