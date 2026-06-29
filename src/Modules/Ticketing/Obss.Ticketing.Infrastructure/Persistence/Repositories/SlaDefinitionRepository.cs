using Microsoft.EntityFrameworkCore;
using Obss.SharedKernel.Infrastructure.Persistence;
using Obss.Ticketing.Application.Abstractions;
using Obss.Ticketing.Domain.Entities;
using Obss.Ticketing.Domain.ValueObjects;

namespace Obss.Ticketing.Infrastructure.Persistence.Repositories;

public sealed class SlaDefinitionRepository : EfRepository<SlaDefinition>, ISlaDefinitionRepository
{
    public SlaDefinitionRepository(TicketDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<SlaDefinition>> GetActiveByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(s => s.TenantId == tenantId && s.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<SlaDefinition?> GetByPriorityAndTenantAsync(string tenantId, TicketPriority priority, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(s => s.TenantId == tenantId && s.Priority == priority && s.IsActive)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
