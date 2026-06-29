using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Notifications.Application.Commands.SendNotification;
using Obss.Ticketing.Domain.Events;

namespace Obss.Notifications.Application.DomainEventHandlers;

public sealed class TicketAssignedNotificationHandler : INotificationHandler<TicketAssignedDomainEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<TicketAssignedNotificationHandler> _logger;

    public TicketAssignedNotificationHandler(
        IMediator mediator,
        ILogger<TicketAssignedNotificationHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(TicketAssignedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling TicketAssignedDomainEvent for ticket {TicketNumber}",
            notification.TicketNumber);

        var command = new SendNotificationCommand(
            TenantId: "default",
            NotificationType: "InApp",
            Channel: "InApp",
            Recipient: notification.AssignedTo,
            Subject: $"Ticket {notification.TicketNumber} Assigned",
            Body: $"Ticket {notification.TicketNumber} has been assigned to you by {notification.AssignedBy}.",
            Priority: "Normal",
            ReferenceType: "Ticket",
            ReferenceId: notification.TicketId);

        await _mediator.Send(command, cancellationToken);
    }
}
