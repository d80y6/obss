using Obss.Rating.Domain.Entities;

namespace Obss.Rating.Domain.DomainServices;

public interface IRatingEngine
{
    RatedUsageResult Rate(UsageRecord record, RatingRule rule);
    IEnumerable<RatedUsageResult> RateBatch(IEnumerable<UsageRecord> records, RatingRule rule);
}
