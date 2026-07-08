using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Orders.Application.IntegrationEvents;
using Obss.Provisioning.Application.Commands.CreateServiceOrder;
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
        var parsedTenantId = Guid.TryParse(tenantId, out var tid) ? tid : Guid.Empty;

        _logger.LogInformation(
            "Creating ServiceOrder for order {OrderId}, item {OrderItemId}",
            notification.OrderId,
            notification.OrderItemId);

        var command = new CreateServiceOrderCommand(
            parsedTenantId,
            notification.OrderId.ToString(),
            $"Provisioning for order {notification.OrderId}",
            null,
            null,
        [
            new CreateServiceOrderItemRequest(
                null,
                notification.Action,
                1,
                $"Service: {notification.ServiceType}",
                null,
                null)
        ]);

        await _mediator.Send(command, cancellationToken);
    }
}
