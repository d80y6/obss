using Obss.Audit.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Audit.Application.Abstractions;

public interface IAuditAlertRepository : IRepository<AuditAlert>
{
    Task<IReadOnlyList<AuditAlert>> GetFilteredAsync(
        string? tenantId,
        string? severity,
        string? alertType,
        bool? isAcknowledged,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditAlert>> GetUnacknowledgedAsync(
        string tenantId,
        CancellationToken cancellationToken = default);

    Task<int> GetCountByTypeAndTimeAsync(
        string tenantId,
        string alertType,
        DateTime since,
        CancellationToken cancellationToken = default);
}
