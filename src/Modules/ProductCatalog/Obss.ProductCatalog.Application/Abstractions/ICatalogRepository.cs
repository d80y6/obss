using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.ProductCatalog.Application.Abstractions;

public interface ICatalogRepository : IRepository<Catalog>
{
    Task<IReadOnlyList<Catalog>> GetFilteredAsync(
        string? searchTerm,
        string? catalogType,
        int offset,
        int limit,
        CancellationToken cancellationToken = default);

    Task<int> GetTotalCountAsync(
        string? searchTerm,
        string? catalogType,
        CancellationToken cancellationToken = default);
}
