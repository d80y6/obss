using Microsoft.EntityFrameworkCore;
using Obss.Audit.Application.Abstractions;
using Obss.Audit.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Audit.Infrastructure.Persistence.Repositories;

public sealed class AuditAlertRepository : EfRepository<AuditAlert>, IAuditAlertRepository
{
    public AuditAlertRepository(AuditDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<AuditAlert>> GetFilteredAsync(
        string? tenantId,
        string? severity,
        string? alertType,
        bool? isAcknowledged,
        DateTime? fromDate,
        DateTime? toDate,
        int offset,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            query = query.Where(a => a.TenantId == tenantId);
        }

        if (!string.IsNullOrWhiteSpace(severity))
        {
            query = query.Where(a => a.Severity.ToString() == severity);
        }

        if (!string.IsNullOrWhiteSpace(alertType))
        {
            query = query.Where(a => a.AlertType.ToString() == alertType);
        }

        if (isAcknowledged.HasValue)
        {
            query = query.Where(a => a.IsAcknowledged == isAcknowledged.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(a => a.DetectedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(a => a.DetectedAt <= toDate.Value);
        }

        query = query
            .OrderByDescending(a => a.DetectedAt)
            .Skip(offset)
            .Take(limit);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditAlert>> GetUnacknowledgedAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.TenantId == tenantId && !a.IsAcknowledged)
            .OrderByDescending(a => a.DetectedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountByTypeAndTimeAsync(
        string tenantId,
        string alertType,
        DateTime since,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .CountAsync(a =>
                a.TenantId == tenantId &&
                a.AlertType.ToString() == alertType &&
                a.DetectedAt >= since,
                cancellationToken);
    }
}
