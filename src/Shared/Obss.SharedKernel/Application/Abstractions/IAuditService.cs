namespace Obss.SharedKernel.Application.Abstractions;

public interface IAuditService
{
    Task LogAsync(
        string entityType,
        string entityId,
        string action,
        string? changes = null,
        string? performedById = null,
        string? performedByName = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? correlationId = null,
        CancellationToken cancellationToken = default);
}
