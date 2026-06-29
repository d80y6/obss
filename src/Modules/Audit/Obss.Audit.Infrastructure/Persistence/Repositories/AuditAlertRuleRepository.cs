using Microsoft.EntityFrameworkCore;
using Obss.Audit.Application.Abstractions;
using Obss.Audit.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Audit.Infrastructure.Persistence.Repositories;

public sealed class AuditAlertRuleRepository : EfRepository<AuditAlertRule>, IAuditAlertRuleRepository
{
    public AuditAlertRuleRepository(AuditDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<AuditAlertRule>> GetActiveRulesAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(r => r.TenantId == tenantId && r.IsActive)
            .ToListAsync(cancellationToken);
    }
}
