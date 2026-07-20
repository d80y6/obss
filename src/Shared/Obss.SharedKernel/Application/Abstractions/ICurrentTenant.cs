namespace Obss.SharedKernel.Application.Abstractions;

public interface ICurrentTenant
{
    string? TenantId { get; }
    string? Name { get; }
    string? ConnectionString { get; }
    bool IsReseller { get; }
    bool IsActive { get; }
}
