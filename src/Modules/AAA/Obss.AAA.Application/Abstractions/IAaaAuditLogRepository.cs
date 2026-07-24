using Obss.AAA.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.AAA.Application.Abstractions;

public interface IAaaAuditLogRepository : IRepository<AaaAuditLog>
{
    Task<(IReadOnlyList<AaaAuditLog> Items, int TotalCount)> GetPaginatedAsync(
        int offset,
        int limit,
        string? eventType = null,
        string? username = null,
        Guid? nasId = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        CancellationToken cancellationToken = default);

    Task<int> CountByDateRangeAsync(
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken = default);
}
