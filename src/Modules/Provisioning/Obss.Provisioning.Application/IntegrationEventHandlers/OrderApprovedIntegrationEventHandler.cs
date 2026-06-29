using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Orders.Application.IntegrationEvents;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Provisioning.Application.IntegrationEventHandlers;

public sealed class ProvisioningRequiredIntegrationEventHandler : INotificationHandler<ProvisioningRequiredIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<ProvisioningRequiredIntegrationEventHandler> _logger;

    public ProvisioningRequiredIntegrationEventHandler(
        IMediator mediator,
        ICurrentTenant currentTenant,
        ILogger<ProvisioningRequiredIntegrationEventHandler> logger)
    {
        _mediator = mediator;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task Handle(ProvisioningRequiredIntegrationEvent notification, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId ?? string.Empty;

        _logger.LogInformation(
            "Creating provisioning job for order {OrderId}, item {OrderItemId}",
            notification.OrderId,
            notification.OrderItemId);

        var command = new Commands.CreateProvisioningJob.CreateProvisioningJobCommand(
            notification.OrderId,
            notification.OrderItemId,
            notification.CustomerId,
            Guid.TryParse(tenantId, out var tid) ? tid : Guid.Empty,
            notification.ServiceType,
            notification.Action);

        await _mediator.Send(command, cancellationToken);
    }
}
