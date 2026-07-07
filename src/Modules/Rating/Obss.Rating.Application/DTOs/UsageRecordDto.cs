namespace Obss.Rating.Application.DTOs;

public sealed record RelatedPartyDto(string PartyId, string PartyName, string Role);

public sealed record UsageRecordDto(
    Guid Id,
    string TenantId,
    Guid SubscriptionId,
    Guid ServiceId,
    string RecordType,
    string UsageType,
    DateTime StartTime,
    DateTime EndTime,
    long Duration,
    long Volume,
    string SourceIdentifier,
    string DestinationIdentifier,
    string Status,
    decimal RatedAmount,
    Guid? RatingRuleId,
    string Currency,
    string? ErrorMessage,
    DateTime RecordedAt,
    DateTime? RatedAt,
    string? Href,
    string? AtType,
    string? AtBaseType,
    string? AtSchemaLocation,
    string? ExternalId,
    List<RelatedPartyDto>? RelatedParties);
