using MediatR;
using Obss.Audit.Application.Commands.CreateAuditEntry;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Audit.Infrastructure.Services;

public sealed class AuditService : IAuditService
{
    private readonly IMediator _mediator;

    public AuditService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task LogAsync(
        string entityType,
        string entityId,
        string action,
        string? changes = null,
        string? performedById = null,
        string? performedByName = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateAuditEntryCommand(
            entityType,
            entityId,
            action,
            changes,
            performedById,
            performedByName,
            ipAddress,
            userAgent,
            correlationId);

        await _mediator.Send(command, cancellationToken);
    }
}
