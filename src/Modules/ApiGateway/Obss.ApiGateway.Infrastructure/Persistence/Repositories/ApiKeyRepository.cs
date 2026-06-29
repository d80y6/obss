using Microsoft.EntityFrameworkCore;
using Obss.ApiGateway.Application.Abstractions;
using Obss.ApiGateway.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ApiGateway.Infrastructure.Persistence.Repositories;

public sealed class ApiKeyRepository : EfRepository<ApiKey>, IApiKeyRepository
{
    public ApiKeyRepository(GatewayDbContext context)
        : base(context)
    {
    }

    public async Task<ApiKey?> GetByKeyAsync(string hashedKey, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(k => k.Key == hashedKey, cancellationToken);
    }

    public async Task<IReadOnlyList<ApiKey>> GetByPartnerIdAsync(Guid partnerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(k => k.PartnerId == partnerId)
            .ToListAsync(cancellationToken);
    }
}
