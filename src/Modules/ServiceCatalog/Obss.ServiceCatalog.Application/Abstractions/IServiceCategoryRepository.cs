using Obss.ServiceCatalog.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.ServiceCatalog.Application.Abstractions;

public interface IServiceCategoryRepository : IRepository<ServiceCategory>
{
    Task<List<ServiceCategory>> GetRootCategoriesAsync(string tenantId, CancellationToken ct = default);
    Task<List<ServiceCategory>> GetChildCategoriesAsync(Guid parentId, CancellationToken ct = default);
    Task<ServiceCategory?> GetByIdWithCandidatesAsync(Guid id, CancellationToken ct = default);
}
