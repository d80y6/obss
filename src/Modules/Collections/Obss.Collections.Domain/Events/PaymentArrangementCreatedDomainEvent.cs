using Obss.SharedKernel.Domain.Common;

namespace Obss.Collections.Domain.Events;

public sealed class PaymentArrangementCreatedDomainEvent : DomainEvent, MediatR.INotification
{
    public PaymentArrangementCreatedDomainEvent(
        Guid arrangementId,
        Guid collectionCaseId,
        Guid customerId,
        decimal totalAmount,
        int installmentCount)
    {
        ArrangementId = arrangementId;
        CollectionCaseId = collectionCaseId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
        InstallmentCount = installmentCount;
    }

    public Guid ArrangementId { get; }
    public Guid CollectionCaseId { get; }
    public Guid CustomerId { get; }
    public decimal TotalAmount { get; }
    public int InstallmentCount { get; }
}
