using Obss.Rating.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Rating.Application.Abstractions;

public interface IUsageRecordRepository : IRepository<UsageRecord>
{
    Task<IReadOnlyList<UsageRecord>> GetUnratedRecordsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UsageRecord>> GetBySubscriptionAsync(
        Guid subscriptionId,
        DateTime? from,
        DateTime? to,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UsageRecord>> GetByDateRangeAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
}
