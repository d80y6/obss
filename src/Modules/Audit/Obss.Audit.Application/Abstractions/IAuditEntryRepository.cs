using Obss.Audit.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Audit.Application.Abstractions;

public interface IAuditEntryRepository : IRepository<AuditEntry>
{
    Task<IReadOnlyList<AuditEntry>> GetFilteredAsync(
        string? tenantId,
        string? entityType,
        string? entityId,
        string? action,
        string? performedById,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditEntry>> GetEntityTrailAsync(
        string tenantId,
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default);

    Task<IDictionary<string, int>> GetCountByActionAsync(
        string? tenantId,
        CancellationToken cancellationToken = default);

    Task<IDictionary<string, int>> GetCountByEntityTypeAsync(
        string? tenantId,
        CancellationToken cancellationToken = default);

    Task<int> DeleteExpiredEntriesAsync(
        Dictionary<string, int> retentionByEntityType,
        CancellationToken cancellationToken = default);

    Task<(int TotalEntries, DateTime? OldestEntry, DateTime? NewestEntry, int SensitiveCount)> GetComplianceSummaryAsync(
        CancellationToken cancellationToken = default);

    Task<int> CountEntriesOlderThanAsync(
        string entityType,
        DateTime cutoffDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditEntry>> GetSensitiveOperationsAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
}
