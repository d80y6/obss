using System.Text.Json;
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
    private readonly IWorkflowStepHandlerRegistry _handlerRegistry;
    private readonly ILogger<WorkflowEngine> _logger;

    public WorkflowEngine(
        IWorkflowDefinitionRepository definitionRepository,
        IWorkflowInstanceRepository instanceRepository,
        IWorkflowStepHandlerRegistry handlerRegistry,
        ILogger<WorkflowEngine> logger)
    {
        _definitionRepository = definitionRepository;
        _instanceRepository = instanceRepository;
        _handlerRegistry = handlerRegistry;
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

        var definition = await _definitionRepository.GetByIdAsync(instance.WorkflowDefinitionId, cancellationToken);
        var step = definition?.Steps.FirstOrDefault(s => s.Id == task.WorkflowStepId);

        task.Start();

        try
        {
            var handler = ResolveHandler(step);

            if (handler is not null)
            {
                var result = await handler.ExecuteAsync(step?.Configuration, cancellationToken);

                if (result.Success)
                {
                    task.Complete(result.ResultData);
                    _logger.LogInformation(
                        "Handler '{HandlerType}' completed step {StepName} for instance {InstanceId}",
                        handler.HandlerType, step?.Name ?? task.StepName, instanceId);
                }
                else
                {
                    task.Fail(result.Error ?? "Step handler returned failure without error message.");
                    _logger.LogError(
                        "Handler '{HandlerType}' failed step {StepName} for instance {InstanceId}: {Error}",
                        handler.HandlerType, step?.Name ?? task.StepName, instanceId, result.Error);
                }
            }
            else
            {
                var resultJson = JsonSerializer.Serialize(new
                {
                    result = "no_handler_available",
                    handlerType = step?.HandlerType,
                    stepType = step?.StepType.ToString(),
                    stepName = step?.Name ?? task.StepName
                });

                task.Complete(resultJson);
                _logger.LogWarning(
                    "No handler found for step {StepName} (handlerType: {HandlerType}, stepType: {StepType}) in instance {InstanceId}. Task auto-completed.",
                    step?.Name ?? task.StepName, step?.HandlerType, step?.StepType, instanceId);
            }
        }
        catch (Exception ex)
        {
            task.Fail(ex.Message);
            _logger.LogError(ex, "Failed to execute step {StepName} for instance {InstanceId}",
                step?.Name ?? task.StepName, instanceId);
        }

        return task;
    }

    private IWorkflowStepHandler? ResolveHandler(WorkflowStep? step)
    {
        if (step is null)
            return null;

        if (!string.IsNullOrEmpty(step.HandlerType))
        {
            var handler = _handlerRegistry.GetHandler(step.HandlerType);
            if (handler is not null)
                return handler;
        }

        return _handlerRegistry.GetHandler(step.StepType.ToString());
    }
}
