using Microsoft.EntityFrameworkCore;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ProductCatalog.Infrastructure.Persistence.Repositories;

public sealed class CatalogRepository : EfRepository<Catalog>, ICatalogRepository
{
    public CatalogRepository(CatalogDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<Catalog>> GetFilteredAsync(
        string? searchTerm,
        string? catalogType,
        int offset,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c =>
                c.Name.Contains(searchTerm) ||
                (c.Description != null && c.Description.Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(catalogType))
        {
            query = query.Where(c => c.CatalogType == catalogType);
        }

        return await query
            .OrderBy(c => c.Name)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(
        string? searchTerm,
        string? catalogType,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c =>
                c.Name.Contains(searchTerm) ||
                (c.Description != null && c.Description.Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(catalogType))
        {
            query = query.Where(c => c.CatalogType == catalogType);
        }

        return await query.CountAsync(cancellationToken);
    }
}
