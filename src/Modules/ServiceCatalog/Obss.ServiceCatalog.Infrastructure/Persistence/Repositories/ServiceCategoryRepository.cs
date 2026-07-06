using Microsoft.EntityFrameworkCore;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ServiceCatalog.Infrastructure.Persistence.Repositories;

internal sealed class ServiceCategoryRepository : EfRepository<ServiceCategory>, IServiceCategoryRepository
{
    public ServiceCategoryRepository(ServiceCatalogDbContext context)
        : base(context)
    {
    }

    public async Task<List<ServiceCategory>> GetRootCategoriesAsync(string tenantId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(c => c.TenantId == tenantId && c.ParentCategoryId == null)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<List<ServiceCategory>> GetChildCategoriesAsync(Guid parentId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(c => c.ParentCategoryId == parentId)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<ServiceCategory?> GetByIdWithCandidatesAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(c => c.Candidates)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }
}
