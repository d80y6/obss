using Obss.ApiGateway.Domain.ValueObjects;

namespace Obss.ApiGateway.Domain.Services;

public interface IRateLimiter
{
    RateLimitResult CheckRateLimit(string apiKey, string path);
}
