namespace Obss.Rating.Domain.DomainServices;

public sealed record RatedUsageResult(
    Guid RecordId,
    decimal Amount,
    string Currency,
    Guid RuleApplied);
