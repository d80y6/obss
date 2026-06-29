using Obss.ProductCatalog.Domain.Domain.Entities;

namespace Obss.ProductCatalog.Application.Abstractions;

public interface IProductConfigurationRepository
{
    Task<List<ProductConfigurationRule>> GetRulesByProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<List<ProductOption>> GetOptionsByProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<ProductConfigurationRule?> GetRuleByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductOption?> GetOptionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductOption?> GetOptionWithValuesByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddRuleAsync(ProductConfigurationRule rule, CancellationToken cancellationToken = default);
    Task AddOptionAsync(ProductOption option, CancellationToken cancellationToken = default);
}
