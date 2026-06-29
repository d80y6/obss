namespace Obss.Rating.Application.DTOs;

public sealed record RatedUsageDto(
    Guid RecordId,
    decimal Amount,
    string Currency,
    Guid RuleApplied);
