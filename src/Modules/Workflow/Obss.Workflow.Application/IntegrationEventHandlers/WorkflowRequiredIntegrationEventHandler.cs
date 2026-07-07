using MediatR;
using Microsoft.Extensions.Logging;
using Obss.SharedKernel.Application.Abstractions;
using Obss.Workflow.Application.Commands.StartWorkflowInstance;
using Obss.Workflow.Application.IntegrationEvents;

namespace Obss.Workflow.Application.IntegrationEventHandlers;

public sealed class WorkflowRequiredIntegrationEventHandler : INotificationHandler<WorkflowRequiredIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly IOutboxService _outboxService;
    private readonly ILogger<WorkflowRequiredIntegrationEventHandler> _logger;

    public WorkflowRequiredIntegrationEventHandler(
        IMediator mediator,
        IOutboxService outboxService,
        ILogger<WorkflowRequiredIntegrationEventHandler> logger)
    {
        _mediator = mediator;
        _outboxService = outboxService;
        _logger = logger;
    }

    public async Task Handle(WorkflowRequiredIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting workflow {WorkflowDefinitionId} for provisioning job {ProvisioningJobId}",
            notification.WorkflowDefinitionId, notification.ProvisioningJobId);

        try
        {
            var result = await _mediator.Send(
                new StartWorkflowInstanceCommand(
                    notification.WorkflowDefinitionId,
                    notification.TriggerEntityType,
                    notification.ProvisioningJobId,
                    notification.CreatedBy),
                cancellationToken);

            if (result.IsSuccess)
            {
                var startedEvent = new WorkflowStartedIntegrationEvent(
                    notification.ProvisioningJobId,
                    result.Value.Id)
                {
                    TenantId = notification.TenantId,
                    CorrelationId = notification.CorrelationId
                };

                await _outboxService.AddAsync(startedEvent, cancellationToken);
                await _mediator.Publish(startedEvent, cancellationToken);

                _logger.LogInformation(
                    "Workflow instance {WorkflowInstanceId} started for provisioning job {ProvisioningJobId}",
                    result.Value.Id, notification.ProvisioningJobId);
            }
            else
            {
                _logger.LogError(
                    "Failed to start workflow {WorkflowDefinitionId} for provisioning job {ProvisioningJobId}: {Error}",
                    notification.WorkflowDefinitionId, notification.ProvisioningJobId, result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error starting workflow {WorkflowDefinitionId} for provisioning job {ProvisioningJobId}",
                notification.WorkflowDefinitionId, notification.ProvisioningJobId);
        }
    }
}
