using Microsoft.Extensions.Logging;
using Obss.CRM.Domain.Events;

namespace Obss.CRM.Application.DomainEventHandlers;

public sealed class CustomerCreatedEventHandler
{
    private readonly ILogger<CustomerCreatedEventHandler> _logger;

    public CustomerCreatedEventHandler(ILogger<CustomerCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(CustomerCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Customer created: {CustomerId} of type {CustomerType} with name {DisplayName} in tenant {TenantId}",
            notification.CustomerId,
            notification.CustomerType,
            notification.DisplayName,
            notification.TenantId);

        return Task.CompletedTask;
    }
}
