using Obss.Audit.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Audit.Application.Abstractions;

public interface IAuditAlertRuleRepository : IRepository<AuditAlertRule>
{
    Task<IReadOnlyList<AuditAlertRule>> GetActiveRulesAsync(
        string tenantId,
        CancellationToken cancellationToken = default);
}
