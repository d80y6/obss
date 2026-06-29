using MediatR;
using Obss.Rating.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Rating.Domain.Entities;

public class UsageRecord : AggregateRoot<Guid>
{
    private UsageRecord() { }

    private UsageRecord(
        Guid id,
        string tenantId,
        Guid subscriptionId,
        Guid serviceId,
        RecordType recordType,
        string usageType,
        DateTime startTime,
        DateTime endTime,
        long duration,
        long volume,
        string sourceIdentifier,
        string destinationIdentifier,
        string currency)
        : base(id)
    {
        TenantId = tenantId;
        SubscriptionId = subscriptionId;
        ServiceId = serviceId;
        RecordType = recordType;
        UsageType = usageType;
        StartTime = startTime;
        EndTime = endTime;
        Duration = duration;
        Volume = volume;
        SourceIdentifier = sourceIdentifier;
        DestinationIdentifier = destinationIdentifier;
        Status = UsageStatus.Unrated;
        Currency = currency;
        RecordedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public Guid SubscriptionId { get; private set; }
    public Guid ServiceId { get; private set; }
    public RecordType RecordType { get; private set; }
    public string UsageType { get; private set; } = string.Empty;
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public long Duration { get; private set; }
    public long Volume { get; private set; }
    public string SourceIdentifier { get; private set; } = string.Empty;
    public string DestinationIdentifier { get; private set; } = string.Empty;
    public UsageStatus Status { get; private set; }
    public decimal RatedAmount { get; private set; }
    public Guid? RatingRuleId { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string? ErrorMessage { get; private set; }
    public DateTime RecordedAt { get; private set; }
    public DateTime? RatedAt { get; private set; }

    public static UsageRecord Create(
        string tenantId,
        Guid subscriptionId,
        Guid serviceId,
        RecordType recordType,
        string usageType,
        DateTime startTime,
        DateTime endTime,
        long duration,
        long volume,
        string sourceIdentifier,
        string destinationIdentifier,
        string currency)
    {
        return new UsageRecord(
            Guid.NewGuid(),
            tenantId,
            subscriptionId,
            serviceId,
            recordType,
            usageType,
            startTime,
            endTime,
            duration,
            volume,
            sourceIdentifier,
            destinationIdentifier,
            currency);
    }

    public void MarkAsRated(decimal amount, Guid ruleId)
    {
        Status = UsageStatus.Rated;
        RatedAmount = amount;
        RatingRuleId = ruleId;
        RatedAt = DateTime.UtcNow;
        ErrorMessage = null;

        AddDomainEvent(new UsageRecordRatedDomainEvent(Id, SubscriptionId, amount, Currency, RecordType));
    }

    public void MarkAsError(string error)
    {
        Status = UsageStatus.Error;
        ErrorMessage = error;
        RatedAt = DateTime.UtcNow;
    }
}

public sealed class UsageRecordRatedDomainEvent : DomainEvent, INotification
{
    public UsageRecordRatedDomainEvent(
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
