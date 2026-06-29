namespace Obss.ApiGateway.Domain.ValueObjects;

public sealed record RateLimitResult(
    bool IsAllowed,
    int RemainingRequests,
    int Limit,
    TimeSpan? RetryAfter = null);
