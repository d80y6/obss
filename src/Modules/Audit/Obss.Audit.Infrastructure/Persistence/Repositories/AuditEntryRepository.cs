using Microsoft.EntityFrameworkCore;
using Obss.Audit.Application.Abstractions;
using Obss.Audit.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Audit.Infrastructure.Persistence.Repositories;

public sealed class AuditEntryRepository : EfRepository<AuditEntry>, IAuditEntryRepository
{
    public AuditEntryRepository(AuditDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<AuditEntry>> GetFilteredAsync(
        string? tenantId,
        string? entityType,
        string? entityId,
        string? action,
        string? performedById,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            query = query.Where(e => e.TenantId == tenantId);
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query = query.Where(e => e.EntityType == entityType);
        }

        if (!string.IsNullOrWhiteSpace(entityId))
        {
            query = query.Where(e => e.EntityId == entityId);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(e => e.Action.ToString() == action);
        }

        if (!string.IsNullOrWhiteSpace(performedById))
        {
            query = query.Where(e => e.PerformedById == performedById);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(e => e.PerformedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(e => e.PerformedAt <= toDate.Value);
        }

        query = query
            .OrderByDescending(e => e.PerformedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditEntry>> GetEntityTrailAsync(
        string tenantId,
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => e.TenantId == tenantId
                     && e.EntityType == entityType
                     && e.EntityId == entityId)
            .OrderByDescending(e => e.PerformedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IDictionary<string, int>> GetCountByActionAsync(
        string? tenantId,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            query = query.Where(e => e.TenantId == tenantId);
        }

        return await query
            .GroupBy(e => e.Action)
            .Select(g => new { Action = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(g => g.Action, g => g.Count, cancellationToken);
    }

    public async Task<IDictionary<string, int>> GetCountByEntityTypeAsync(
        string? tenantId,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            query = query.Where(e => e.TenantId == tenantId);
        }

        return await query
            .GroupBy(e => e.EntityType)
            .Select(g => new { EntityType = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.EntityType, g => g.Count, cancellationToken);
    }

    public async Task<int> DeleteExpiredEntriesAsync(
        Dictionary<string, int> retentionByEntityType,
        CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow;
        var totalDeleted = 0;

        foreach (var (entityType, retentionDays) in retentionByEntityType)
        {
            var expiryDate = cutoffDate.AddDays(-retentionDays);

            var expired = await DbSet
                .Where(e => e.EntityType == entityType && e.PerformedAt < expiryDate)
                .ToListAsync(cancellationToken);

            if (expired.Count > 0)
            {
                DbSet.RemoveRange(expired);
                totalDeleted += expired.Count;
            }
        }

        return totalDeleted;
    }

    public async Task<(int TotalEntries, DateTime? OldestEntry, DateTime? NewestEntry, int SensitiveCount)> GetComplianceSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var totalEntries = await DbSet.CountAsync(cancellationToken);
        var oldestEntry = await DbSet.MinAsync(e => (DateTime?)e.PerformedAt, cancellationToken);
        var newestEntry = await DbSet.MaxAsync(e => (DateTime?)e.PerformedAt, cancellationToken);
        var sensitiveCount = await DbSet.CountAsync(e => e.IsSensitive, cancellationToken);
        return (totalEntries, oldestEntry, newestEntry, sensitiveCount);
    }

    public async Task<int> CountEntriesOlderThanAsync(
        string entityType,
        DateTime cutoffDate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(e => e.EntityType == entityType && e.PerformedAt < cutoffDate, cancellationToken);
    }

    public async Task<IReadOnlyList<AuditEntry>> GetSensitiveOperationsAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => e.IsSensitive && e.PerformedAt >= from && e.PerformedAt <= to)
            .OrderByDescending(e => e.PerformedAt)
            .ToListAsync(cancellationToken);
    }
}
