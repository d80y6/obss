using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Audit.Domain.Events;

namespace Obss.Audit.Application.DomainEventHandlers;

public sealed class AuditEntryCreatedEventHandler : INotificationHandler<AuditEntryCreatedDomainEvent>
{
    private readonly ILogger<AuditEntryCreatedEventHandler> _logger;

    public AuditEntryCreatedEventHandler(ILogger<AuditEntryCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(AuditEntryCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        if (notification.IsSensitive)
        {
            _logger.LogWarning(
                "Sensitive audit entry created: {Action} on {EntityType}({EntityId}) by user {UserId}",
                notification.Action,
                notification.EntityType,
                notification.EntityId,
                notification.PerformedById);
        }
        else
        {
            _logger.LogInformation(
                "Audit entry created: {Action} on {EntityType}({EntityId})",
                notification.Action,
                notification.EntityType,
                notification.EntityId);
        }

        return Task.CompletedTask;
    }
}
