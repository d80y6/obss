using Obss.ModuleTemplate.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.ModuleTemplate.Application.Abstractions;

public interface ISampleRepository : IRepository<SampleAggregate>
{
    Task<SampleAggregate?> GetByNameAsync(string name, string tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SampleAggregate>> GetFilteredAsync(
        string? tenantId,
        bool? isActive,
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
