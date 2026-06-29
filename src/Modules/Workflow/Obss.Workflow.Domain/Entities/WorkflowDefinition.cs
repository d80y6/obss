using Obss.SharedKernel.Domain.Common;
using Obss.Workflow.Domain.ValueObjects;

namespace Obss.Workflow.Domain.Entities;

public class WorkflowDefinition : AggregateRoot<Guid>
{
    private readonly List<WorkflowStep> _steps = [];

    private WorkflowDefinition() { }

    private WorkflowDefinition(
        Guid id,
        string tenantId,
        string name,
        string? description,
        WorkflowCategory category,
        int version)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        Category = category;
        IsActive = true;
        Version = version;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public WorkflowCategory Category { get; private set; }
    public bool IsActive { get; private set; }
    public int Version { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<WorkflowStep> Steps => _steps.AsReadOnly();

    public static WorkflowDefinition Create(
        string tenantId,
        string name,
        string? description,
        WorkflowCategory category)
    {
        return new WorkflowDefinition(
            Guid.NewGuid(),
            tenantId,
            name,
            description,
            category,
            1);
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public WorkflowDefinition CreateNewVersion()
    {
        var newVersion = new WorkflowDefinition(
            Guid.NewGuid(),
            TenantId,
            Name,
            Description,
            Category,
            Version + 1);

        foreach (var step in _steps)
        {
            newVersion.AddStep(step.StepNumber, step.Name, step.Description, step.StepType,
                step.HandlerType, step.Configuration, step.Timeout, step.RetryCount,
                step.RetryDelaySeconds, step.IsRequired);
        }

        return newVersion;
    }

    public void AddStep(int stepNumber, string name, string? description, StepType stepType,
        string? handlerType, string? configuration, int timeout, int retryCount,
        int retryDelaySeconds, bool isRequired)
    {
        var step = new WorkflowStep(
            Guid.NewGuid(),
            Id,
            stepNumber,
            name,
            description,
            stepType,
            handlerType,
            configuration,
            timeout,
            retryCount,
            retryDelaySeconds,
            isRequired);

        _steps.Add(step);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveStep(Guid stepId)
    {
        var step = _steps.FirstOrDefault(s => s.Id == stepId);
        if (step is null)
            return;

        _steps.Remove(step);
        UpdatedAt = DateTime.UtcNow;
    }
}
