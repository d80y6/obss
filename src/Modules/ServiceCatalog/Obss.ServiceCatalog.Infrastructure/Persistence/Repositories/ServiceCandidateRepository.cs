using Microsoft.EntityFrameworkCore;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ServiceCatalog.Infrastructure.Persistence.Repositories;

internal sealed class ServiceCandidateRepository : EfRepository<ServiceCandidate>, IServiceCandidateRepository
{
    public ServiceCandidateRepository(ServiceCatalogDbContext context)
        : base(context)
    {
    }

    public async Task<List<ServiceCandidate>> GetByCategoryAsync(Guid categoryId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(c => c.Categories.Any(cat => cat.Id == categoryId))
            .ToListAsync(ct);
    }

    public async Task<ServiceCandidate?> GetByIdWithCategoriesAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(c => c.Categories)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }
}
