using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using ProductSpecification = Obss.ProductCatalog.Domain.Domain.Entities.ProductSpecification;

namespace Obss.ProductCatalog.Application.Abstractions;

public interface IProductSpecificationRepository : IRepository<ProductSpecification>
{
    Task<ProductSpecification?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<ProductSpecification> Items, int TotalCount)> GetFilteredAsync(
        string? searchTerm,
        LifecycleStatus? status,
        string? brand,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
