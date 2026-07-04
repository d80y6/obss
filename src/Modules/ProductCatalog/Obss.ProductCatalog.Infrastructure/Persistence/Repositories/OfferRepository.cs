using Microsoft.EntityFrameworkCore;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ProductCatalog.Infrastructure.Persistence.Repositories;

public sealed class OfferRepository : EfRepository<Offer>, IOfferRepository
{
    public OfferRepository(CatalogDbContext context)
        : base(context)
    {
    }

    public async Task<Offer?> GetByIdWithPricingsAsync(Guid offerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.OfferPricings)
            .Include(o => o.Discounts)
            .FirstOrDefaultAsync(o => o.Id == offerId, cancellationToken);
    }

    public async Task<Offer?> GetByIdWithTermsAsync(Guid offerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.Terms)
            .FirstOrDefaultAsync(o => o.Id == offerId, cancellationToken);
    }

    public async Task<IReadOnlyList<Offer>> GetActiveOffersAsync(
        OfferType? offerType,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(o => o.OfferPricings)
            .Include(o => o.Discounts)
            .AsQueryable();

        if (offerType.HasValue)
        {
            query = query.Where(o => o.OfferType == offerType.Value);
        }

        return await query
            .OrderBy(o => o.SortOrder)
            .ThenBy(o => o.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Offer> Items, int TotalCount)> GetFilteredAsync(
        OfferType? offerType,
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(o => o.OfferPricings)
            .Include(o => o.Discounts)
            .AsQueryable();

        if (offerType.HasValue)
        {
            query = query.Where(o => o.OfferType == offerType.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(o =>
                o.Name.Contains(searchTerm) ||
                (o.Description != null && o.Description.Contains(searchTerm)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(o => o.SortOrder)
            .ThenBy(o => o.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
