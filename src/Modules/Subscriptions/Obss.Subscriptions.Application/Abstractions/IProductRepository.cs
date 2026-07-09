using Obss.SharedKernel.Application.Abstractions;
using Obss.Subscriptions.Domain.Entities;

namespace Obss.Subscriptions.Application.Abstractions;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Product>> GetListAsync(CancellationToken ct = default);
}
