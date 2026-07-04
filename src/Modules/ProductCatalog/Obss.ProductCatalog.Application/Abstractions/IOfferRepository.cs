using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.ProductCatalog.Application.Abstractions;

public interface IOfferRepository : IRepository<Offer>
{
    Task<Offer?> GetByIdWithPricingsAsync(Guid offerId, CancellationToken cancellationToken = default);
    Task<Offer?> GetByIdWithTermsAsync(Guid offerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Offer>> GetActiveOffersAsync(
        OfferType? offerType,
        CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Offer> Items, int TotalCount)> GetFilteredAsync(
        OfferType? offerType,
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
