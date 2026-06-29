using Obss.SharedKernel.Domain.Common;

namespace Obss.Collections.Domain.Events;

public sealed class PaymentArrangementDefaultedDomainEvent : DomainEvent, MediatR.INotification
{
    public PaymentArrangementDefaultedDomainEvent(
        Guid arrangementId,
        Guid collectionCaseId,
        Guid customerId,
        decimal totalAmount,
        decimal paidAmount)
    {
        ArrangementId = arrangementId;
        CollectionCaseId = collectionCaseId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
        PaidAmount = paidAmount;
    }

    public Guid ArrangementId { get; }
    public Guid CollectionCaseId { get; }
    public Guid CustomerId { get; }
    public decimal TotalAmount { get; }
    public decimal PaidAmount { get; }
}
