using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.Workflow.Application.IntegrationEvents;

namespace Obss.Provisioning.Application.IntegrationEventHandlers;

public sealed class WorkflowStartedIntegrationEventHandler : INotificationHandler<WorkflowStartedIntegrationEvent>
{
    private readonly IProvisioningJobRepository _jobRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WorkflowStartedIntegrationEventHandler> _logger;

    public WorkflowStartedIntegrationEventHandler(
        IProvisioningJobRepository jobRepository,
        IUnitOfWork unitOfWork,
        ILogger<WorkflowStartedIntegrationEventHandler> logger)
    {
        _jobRepository = jobRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(WorkflowStartedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Assigning workflow instance {WorkflowInstanceId} to provisioning job {ProvisioningJobId}",
            notification.WorkflowInstanceId, notification.ProvisioningJobId);

        var job = await _jobRepository.GetByIdAsync(notification.ProvisioningJobId, cancellationToken);
        if (job is null)
        {
            _logger.LogWarning(
                "Provisioning job {ProvisioningJobId} not found for workflow assignment",
                notification.ProvisioningJobId);
            return;
        }

        job.AssignWorkflow(notification.WorkflowInstanceId);
        job.Queue();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Provisioning job {ProvisioningJobId} queued with workflow instance {WorkflowInstanceId}",
            notification.ProvisioningJobId, notification.WorkflowInstanceId);
    }
}
