namespace Obss.SharedKernel.Application.Abstractions;

public interface ITenantStore
{
    Task<TenantInfo?> GetTenantAsync(string tenantId, CancellationToken cancellationToken = default);
}

public sealed record TenantInfo(
    string Id,
    string Name,
    bool IsActive,
    bool IsReseller,
    string? ConnectionString = null);
