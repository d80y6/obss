using Microsoft.EntityFrameworkCore;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;
using ProductSpecification = Obss.ProductCatalog.Domain.Domain.Entities.ProductSpecification;

namespace Obss.ProductCatalog.Infrastructure.Persistence.Repositories;

public sealed class ProductSpecificationRepository : EfRepository<ProductSpecification>, IProductSpecificationRepository
{
    public ProductSpecificationRepository(CatalogDbContext context)
        : base(context)
    {
    }

    public async Task<ProductSpecification?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(ps => ps.Characteristics)
                .ThenInclude(c => c.Values)
            .Include(ps => ps.Relationships)
            .FirstOrDefaultAsync(ps => ps.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<ProductSpecification> Items, int TotalCount)> GetFilteredAsync(
        string? searchTerm,
        LifecycleStatus? status,
        string? brand,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(ps => ps.Characteristics)
                .ThenInclude(c => c.Values)
            .Include(ps => ps.Relationships)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(ps => ps.LifecycleStatus == status.Value);

        if (!string.IsNullOrWhiteSpace(brand))
            query = query.Where(ps => ps.Brand != null && ps.Brand.Contains(brand));

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(ps =>
                ps.Name.Contains(searchTerm) ||
                (ps.Description != null && ps.Description.Contains(searchTerm)));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(ps => ps.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
