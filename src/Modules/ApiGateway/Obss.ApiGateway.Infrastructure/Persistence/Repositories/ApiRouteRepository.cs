using Microsoft.EntityFrameworkCore;
using Obss.ApiGateway.Application.Abstractions;
using Obss.ApiGateway.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ApiGateway.Infrastructure.Persistence.Repositories;

public sealed class ApiRouteRepository : EfRepository<ApiRoute>, IApiRouteRepository
{
    public ApiRouteRepository(GatewayDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<ApiRoute>> GetActiveRoutesAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(r => r.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<ApiRoute?> GetByPathAndMethodAsync(string path, string method, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(r => r.Path == path && r.Method == method, cancellationToken);
    }
}
