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

    public async Task<IReadOnlyList<Offer>> GetActiveOffersAsync(
        OfferType? offerType,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(o => o.OfferPricings)
            .Include(o => o.Discounts)
            .Where(o => o.IsActive)
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
}
