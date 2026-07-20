using Obss.SharedKernel.Application.Abstractions;

namespace Obss.SharedKernel.Infrastructure.Services;

public sealed class DefaultTenantStore : ITenantStore
{
    public Task<TenantInfo?> GetTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<TenantInfo?>(null);
    }
}
