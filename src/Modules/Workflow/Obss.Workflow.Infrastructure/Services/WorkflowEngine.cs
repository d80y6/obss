using Microsoft.Extensions.Logging;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Domain.Entities;
using Obss.Workflow.Domain.Services;
using Obss.Workflow.Domain.ValueObjects;

namespace Obss.Workflow.Infrastructure.Services;

public sealed class WorkflowEngine : IWorkflowEngine
{
    private readonly IWorkflowDefinitionRepository _definitionRepository;
    private readonly IWorkflowInstanceRepository _instanceRepository;
    private readonly ILogger<WorkflowEngine> _logger;

    public WorkflowEngine(
        IWorkflowDefinitionRepository definitionRepository,
        IWorkflowInstanceRepository instanceRepository,
        ILogger<WorkflowEngine> logger)
    {
        _definitionRepository = definitionRepository;
        _instanceRepository = instanceRepository;
        _logger = logger;
    }

    public async Task<WorkflowInstance> StartWorkflow(Guid definitionId, string entityType, Guid entityId, string createdBy, CancellationToken cancellationToken = default)
    {
        var definition = await _definitionRepository.GetByIdAsync(definitionId, cancellationToken);
        if (definition is null)
            throw new InvalidOperationException($"Workflow definition '{definitionId}' not found.");

        var instance = WorkflowInstance.Create(
            definitionId,
            definition.Name,
            entityType,
            entityId,
            createdBy);

        instance.Start();

        foreach (var step in definition.Steps.OrderBy(s => s.StepNumber))
        {
            var task = new WorkflowTaskInstance(
                Guid.NewGuid(),
                instance.Id,
                step.Id,
                step.Name,
                step.StepType == StepType.Manual ? null : "system");

            instance.AddTask(task);
        }

        await _instanceRepository.AddAsync(instance, cancellationToken);
        _logger.LogInformation("Started workflow instance {InstanceId} for definition {DefinitionId}", instance.Id, definitionId);

        return instance;
    }

    public async Task<WorkflowTaskInstance> ExecuteStep(Guid instanceId, Guid taskId, CancellationToken cancellationToken = default)
    {
        var instance = await _instanceRepository.GetByIdAsync(instanceId, cancellationToken);
        if (instance is null)
            throw new InvalidOperationException($"Workflow instance '{instanceId}' not found.");

        var task = instance.Tasks.FirstOrDefault(t => t.Id == taskId);
        if (task is null)
            throw new InvalidOperationException($"Task '{taskId}' not found in instance '{instanceId}'.");

        task.Start();

        try
        {
            await Task.Delay(100, cancellationToken);
            task.Complete("{\"result\": \"executed\"}");
            _logger.LogInformation("Executed workflow task {TaskId} for instance {InstanceId}", taskId, instanceId);
        }
        catch (Exception ex)
        {
            task.Fail(ex.Message);
            _logger.LogError(ex, "Failed to execute workflow task {TaskId} for instance {InstanceId}", taskId, instanceId);
        }

        return task;
    }
}
