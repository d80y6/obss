using Obss.SharedKernel.Application.Abstractions;
using Obss.Ticketing.Domain.Entities;

namespace Obss.Ticketing.Application.Abstractions;

public interface ISlaDefinitionRepository : IRepository<SlaDefinition>
{
    Task<IReadOnlyList<SlaDefinition>> GetActiveByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<SlaDefinition?> GetByPriorityAndTenantAsync(string tenantId, Domain.ValueObjects.TicketPriority priority, CancellationToken cancellationToken = default);
}
