using MediatR;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Rating.Domain.Events;

public sealed class PromotionAppliedDomainEvent : DomainEvent, INotification
{
    public PromotionAppliedDomainEvent(
        Guid promotionId,
        string promotionName,
        decimal discount,
        string currency,
        Guid? subscriptionId,
        Guid? recordId)
    {
        PromotionId = promotionId;
        PromotionName = promotionName;
        Discount = discount;
        Currency = currency;
        SubscriptionId = subscriptionId;
        RecordId = recordId;
    }

    public Guid PromotionId { get; }
    public string PromotionName { get; }
    public decimal Discount { get; }
    public string Currency { get; }
    public Guid? SubscriptionId { get; }
    public Guid? RecordId { get; }
}
