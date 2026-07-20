using Microsoft.EntityFrameworkCore;
using Obss.IAM.Infrastructure.Persistence;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.IAM.Infrastructure.Services;

public sealed class IamTenantStore : ITenantStore
{
    private readonly IamDbContext _dbContext;

    public IamTenantStore(IamDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TenantInfo?> GetTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(tenantId, out var guid))
            return null;

        var tenant = await _dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == guid, cancellationToken);

        if (tenant is null)
            return null;

        return new TenantInfo(
            tenant.Id.ToString("N"),
            tenant.Name,
            tenant.IsActive,
            false,
            tenant.ConnectionString);
    }
}
