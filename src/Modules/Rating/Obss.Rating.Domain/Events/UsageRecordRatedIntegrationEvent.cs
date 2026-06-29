using Obss.Rating.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Events;

namespace Obss.Rating.Domain.Events;

public sealed class UsageRecordRatedIntegrationEvent : IntegrationEvent
{
    public UsageRecordRatedIntegrationEvent(
        Guid recordId,
        Guid subscriptionId,
        decimal amount,
        string currency,
        RecordType recordType)
    {
        RecordId = recordId;
        SubscriptionId = subscriptionId;
        Amount = amount;
        Currency = currency;
        RecordType = recordType;
    }

    public Guid RecordId { get; }
    public Guid SubscriptionId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public RecordType RecordType { get; }
}
