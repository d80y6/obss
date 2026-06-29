using Microsoft.EntityFrameworkCore;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ProductCatalog.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository : EfRepository<Category>, ICategoryRepository
{
    public CategoryRepository(CatalogDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
}
