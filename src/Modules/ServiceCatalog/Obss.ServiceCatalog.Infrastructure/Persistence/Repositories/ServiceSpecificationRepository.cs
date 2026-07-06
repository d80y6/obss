using Microsoft.EntityFrameworkCore;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ServiceCatalog.Infrastructure.Persistence.Repositories;

internal sealed class ServiceSpecificationRepository : EfRepository<ServiceSpecification>, IServiceSpecificationRepository
{
    public ServiceSpecificationRepository(ServiceCatalogDbContext context)
        : base(context)
    {
    }

    public async Task<ServiceSpecification?> GetByIdWithCharacteristicsAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(s => s.Characteristics)
                .ThenInclude(c => c.Values)
            .Include(s => s.Relationships)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<List<ServiceSpecification>> GetByBrandAsync(string brand, CancellationToken ct = default)
    {
        return await DbSet
            .Where(s => s.Brand == brand)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
    }
}
