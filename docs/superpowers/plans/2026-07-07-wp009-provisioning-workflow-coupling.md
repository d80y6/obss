# WP-009: Fix Provisioning→Workflow Coupling

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Replace the Provisioning module's direct MediatR command dispatch to the Workflow module with an integration event pattern, removing the compile-time cross-module command dependency.

**Architecture:** Use a saga of two integration events: `WorkflowRequiredIntegrationEvent` (Provisioning → Workflow) and `WorkflowStartedIntegrationEvent` (Workflow → Provisioning). Both events defined in the Workflow module. Provisioning keeps the project reference to Workflow.Application for event types only (same pattern as Provisioning → Orders).

**Tech Stack:** .NET 9, MediatR, SharedKernel (IntegrationEvent, IOutboxService)

---

### File Structure

| File | Action | Responsibility |
|------|--------|----------------|
| `Obss.Workflow.Application/IntegrationEvents/WorkflowRequiredIntegrationEvent.cs` | Create | Event: Provisioning requests workflow start |
| `Obss.Workflow.Application/IntegrationEvents/WorkflowStartedIntegrationEvent.cs` | Create | Event: Workflow confirms instance started |
| `Obss.Workflow.Application/IntegrationEventHandlers/WorkflowRequiredIntegrationEventHandler.cs` | Create | Handler: starts workflow, publishes started event |
| `Obss.Provisioning.Application/IntegrationEventHandlers/WorkflowStartedIntegrationEventHandler.cs` | Create | Handler: assigns workflow ID, queues job |
| `Obss.Provisioning.Application/Commands/CreateProvisioningJob/CreateProvisioningJobCommandHandler.cs` | Modify | Remove direct MediatR call to Workflow; publish event instead |
| `Obss.Provisioning.Application/IntegrationEvents/` | Ensure dir exists | Already has `ProvisioningJobCompletedIntegrationEvent` |

---

### Task 1: Define integration events in Workflow module

**Files:**
- Create: `src/Modules/Workflow/Obss.Workflow.Application/IntegrationEvents/WorkflowRequiredIntegrationEvent.cs`
- Create: `src/Modules/Workflow/Obss.Workflow.Application/IntegrationEvents/WorkflowStartedIntegrationEvent.cs`

**Step 1.1: Create WorkflowRequiredIntegrationEvent**

`WorkflowRequiredIntegrationEvent` carries the data the Workflow module needs to start a workflow instance for a provisioning job.

```csharp
using Obss.SharedKernel.Domain.Events;

namespace Obss.Workflow.Application.IntegrationEvents;

public sealed class WorkflowRequiredIntegrationEvent : IntegrationEvent
{
    public WorkflowRequiredIntegrationEvent(
        Guid provisioningJobId,
        Guid workflowDefinitionId,
        string triggerEntityType,
        string createdBy)
    {
        ProvisioningJobId = provisioningJobId;
        WorkflowDefinitionId = workflowDefinitionId;
        TriggerEntityType = triggerEntityType;
        CreatedBy = createdBy;
    }

    public Guid ProvisioningJobId { get; }
    public Guid WorkflowDefinitionId { get; }
    public string TriggerEntityType { get; }
    public string CreatedBy { get; }
}
```

**Step 1.2: Create WorkflowStartedIntegrationEvent**

`WorkflowStartedIntegrationEvent` carries the result back to Provisioning.

```csharp
using Obss.SharedKernel.Domain.Events;

namespace Obss.Workflow.Application.IntegrationEvents;

public sealed class WorkflowStartedIntegrationEvent : IntegrationEvent
{
    public WorkflowStartedIntegrationEvent(
        Guid provisioningJobId,
        Guid workflowInstanceId)
    {
        ProvisioningJobId = provisioningJobId;
        WorkflowInstanceId = workflowInstanceId;
    }

    public Guid ProvisioningJobId { get; }
    public Guid WorkflowInstanceId { get; }
}
```

---

### Task 2: Create WorkflowRequiredIntegrationEventHandler in Workflow module

**Files:**
- Create: `src/Modules/Workflow/Obss.Workflow.Application/IntegrationEventHandlers/WorkflowRequiredIntegrationEventHandler.cs`

This handler receives the event, starts the workflow using the existing internal command, and publishes `WorkflowStartedIntegrationEvent`.

```csharp
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Domain.Events;
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
```

---

### Task 3: Create WorkflowStartedIntegrationEventHandler in Provisioning module

**Files:**
- Create: `src/Modules/Provisioning/Obss.Provisioning.Application/IntegrationEventHandlers/WorkflowStartedIntegrationEventHandler.cs`

This handler receives the confirmation from Workflow and updates the provisioning job.

```csharp
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
    }
}
```

---

### Task 4: Modify CreateProvisioningJobCommandHandler

**Files:**
- Modify: `src/Modules/Provisioning/Obss.Provisioning.Application/Commands/CreateProvisioningJob/CreateProvisioningJobCommandHandler.cs`

Remove the direct MediatR command to Workflow. Instead, publish `WorkflowRequiredIntegrationEvent`. Keep the existing template check logic for when to publish.

```csharp
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Application.DTOs;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.IntegrationEvents;

namespace Obss.Provisioning.Application.Commands.CreateProvisioningJob;

public sealed class CreateProvisioningJobCommandHandler : IRequestHandler<CreateProvisioningJobCommand, Result<ProvisioningJobDto>>
{
    private readonly IProvisioningJobRepository _jobRepository;
    private readonly IProvisioningTemplateRepository _templateRepository;
    private readonly IOutboxService _outboxService;
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateProvisioningJobCommandHandler> _logger;

    public CreateProvisioningJobCommandHandler(
        IProvisioningJobRepository jobRepository,
        IProvisioningTemplateRepository templateRepository,
        IOutboxService outboxService,
        IMediator mediator,
        IUnitOfWork unitOfWork,
        ILogger<CreateProvisioningJobCommandHandler> logger)
    {
        _jobRepository = jobRepository;
        _templateRepository = templateRepository;
        _outboxService = outboxService;
        _mediator = mediator;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ProvisioningJobDto>> Handle(CreateProvisioningJobCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ProvisioningAction>(request.Action, out var action))
            return Result.Failure<ProvisioningJobDto>(Error.Validation($"Invalid action: '{request.Action}'."));

        var job = ProvisioningJob.Create(
            request.TenantId,
            request.OrderId,
            request.OrderItemId,
            request.CustomerId,
            request.ServiceType,
            action);

        var template = await _templateRepository.GetByServiceTypeAndActionAsync(
            request.ServiceType, request.Action, cancellationToken);

        if (template is not null && template.IsActive)
        {
            var integrationEvent = new WorkflowRequiredIntegrationEvent(
                job.Id,
                template.WorkflowDefinitionId,
                "ProvisioningJob",
                "system")
            {
                TenantId = request.TenantId,
                CorrelationId = job.CorrelationId
            };

            await _outboxService.AddAsync(integrationEvent, cancellationToken);
            await _mediator.Publish(integrationEvent, cancellationToken);
        }

        job.Queue();
        await _jobRepository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created provisioning job {JobId} for order {OrderId}, service type {ServiceType}",
            job.Id, request.OrderId, request.ServiceType);

        return Result.Success(job.Adapt<ProvisioningJobDto>());
    }
}
```

Note: The event handler runs synchronously via `_mediator.Publish()` BEFORE `SaveChangesAsync`. The `WorkflowStartedIntegrationEventHandler` loads the job from DB but it hasn't been saved yet. This is a problem.

**Fix:** Move `SaveChangesAsync` before `_mediator.Publish(integrationEvent)` so the job exists in the database when the event handler runs.

But then the event handler queues the job (calls `Queue()`) before the command handler queues it. Let me adjust: the command handler should NOT queue the job when a workflow is required. The event handler handles that.

Revised flow:
1. If template exists: create job, save (Pending), publish event
2. If no template: create job, queue, save
3. Event handler: load job, assign workflow, queue, save

```csharp
public async Task<Result<ProvisioningJobDto>> Handle(CreateProvisioningJobCommand request, CancellationToken cancellationToken)
{
    if (!Enum.TryParse<ProvisioningAction>(request.Action, out var action))
        return Result.Failure<ProvisioningJobDto>(Error.Validation($"Invalid action: '{request.Action}'."));

    var job = ProvisioningJob.Create(
        request.TenantId,
        request.OrderId,
        request.OrderItemId,
        request.CustomerId,
        request.ServiceType,
        action);

    var template = await _templateRepository.GetByServiceTypeAndActionAsync(
        request.ServiceType, request.Action, cancellationToken);

    if (template is not null && template.IsActive)
    {
        await _jobRepository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var integrationEvent = new WorkflowRequiredIntegrationEvent(
            job.Id,
            template.WorkflowDefinitionId,
            "ProvisioningJob",
            "system")
        {
            TenantId = request.TenantId,
            CorrelationId = job.CorrelationId
        };

        await _outboxService.AddAsync(integrationEvent, cancellationToken);
        await _mediator.Publish(integrationEvent, cancellationToken);

        _logger.LogInformation(
            "Created provisioning job {JobId} with pending workflow for order {OrderId}",
            job.Id, request.OrderId);
    }
    else
    {
        job.Queue();
        await _jobRepository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created provisioning job {JobId} for order {OrderId} (no workflow)",
            job.Id, request.OrderId);
    }

    return Result.Success(job.Adapt<ProvisioningJobDto>());
}
```

---

### Task 5: Build and verify

**Run:** `dotnet build src/Host/Obss.Host/Obss.Host.csproj --configuration Release --no-restore`
**Expected:** Build succeeded, 0 warnings, 0 errors

**Run:** `dotnet test tests/Obss.SharedKernel.Tests/ --configuration Release --no-restore`
**Expected:** Passed!  - Failed: 0, Passed: 74
