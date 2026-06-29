using Obss.SharedKernel.Domain.Events;

namespace Obss.Payments.Application.IntegrationEvents;

public sealed class PaymentRefundedIntegrationEvent : IntegrationEvent
{
    public PaymentRefundedIntegrationEvent(
        Guid paymentId,
        string paymentNumber,
        decimal amount,
        string reason,
        string tenantId)
    {
        PaymentId = paymentId;
        PaymentNumber = paymentNumber;
        Amount = amount;
        Reason = reason;
        TenantId = tenantId;
    }

    public Guid PaymentId { get; }
    public string PaymentNumber { get; }
    public decimal Amount { get; }
    public string Reason { get; }
}
