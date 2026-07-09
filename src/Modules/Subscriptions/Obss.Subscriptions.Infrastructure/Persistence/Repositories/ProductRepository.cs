using Microsoft.EntityFrameworkCore;
using Obss.SharedKernel.Infrastructure.Persistence;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Domain.Entities;

namespace Obss.Subscriptions.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository : EfRepository<Product>, IProductRepository
{
    public ProductRepository(SubscriptionDbContext context)
        : base(context)
    {
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<List<Product>> GetListAsync(CancellationToken ct = default)
        => await DbSet.ToListAsync(ct);
}
