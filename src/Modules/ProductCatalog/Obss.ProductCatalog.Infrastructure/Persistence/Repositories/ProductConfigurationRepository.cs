using Microsoft.EntityFrameworkCore;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Domain.Domain.Entities;

namespace Obss.ProductCatalog.Infrastructure.Persistence.Repositories;

public sealed class ProductConfigurationRepository : IProductConfigurationRepository
{
    private readonly CatalogDbContext _context;

    public ProductConfigurationRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductConfigurationRule>> GetRulesByProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.ProductConfigurationRules
            .Where(r => r.ProductId == productId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ProductOption>> GetOptionsByProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.ProductOptions
            .Include(o => o.Values)
            .Where(o => o.ProductId == productId)
            .OrderBy(o => o.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductConfigurationRule?> GetRuleByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ProductConfigurationRules
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<ProductOption?> GetOptionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ProductOptions
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<ProductOption?> GetOptionWithValuesByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ProductOptions
            .Include(o => o.Values)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task AddRuleAsync(ProductConfigurationRule rule, CancellationToken cancellationToken = default)
    {
        await _context.ProductConfigurationRules.AddAsync(rule, cancellationToken);
    }

    public async Task AddOptionAsync(ProductOption option, CancellationToken cancellationToken = default)
    {
        await _context.ProductOptions.AddAsync(option, cancellationToken);
    }
}
