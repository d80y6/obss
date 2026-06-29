using Obss.SharedKernel.Domain.Events;

namespace Obss.Billing.Application.IntegrationEvents;

public sealed class BillFinalizedIntegrationEvent : IntegrationEvent
{
    public BillFinalizedIntegrationEvent(Guid billId, Guid customerId, decimal grandTotal, string currency)
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
