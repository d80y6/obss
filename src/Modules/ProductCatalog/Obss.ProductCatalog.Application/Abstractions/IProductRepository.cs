using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.ProductCatalog.Application.Abstractions;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetByIdWithOffersAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetFilteredAsync(
        Guid? categoryId,
        ProductType? productType,
        LifecycleStatus? status,
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
