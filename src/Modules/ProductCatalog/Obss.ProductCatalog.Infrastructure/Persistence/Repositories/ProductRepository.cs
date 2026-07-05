using Microsoft.EntityFrameworkCore;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ProductCatalog.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository : EfRepository<Product>, IProductRepository
{
    public ProductRepository(CatalogDbContext context)
        : base(context)
    {
    }

    public async Task<Product?> GetByIdWithOffersAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Category)
            .Include(p => p.ProductOffers)
                .ThenInclude(po => po.Offer)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetFilteredAsync(
        Guid? categoryId,
        ProductType? productType,
        LifecycleStatus? status,
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(p => p.Category)
            .Include(p => p.ProductOffers)
                .ThenInclude(po => po.Offer)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (productType.HasValue)
        {
            query = query.Where(p => p.ProductType == productType.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.LifecycleStatus == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p =>
                p.Name.Contains(searchTerm) ||
                (p.Description != null && p.Description.Contains(searchTerm)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
