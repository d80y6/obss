using Obss.SharedKernel.Domain.Common;
using Obss.Workflow.Domain.ValueObjects;

namespace Obss.Workflow.Domain.Entities;

public class WorkflowStep : Entity<Guid>
{
    private WorkflowStep() { }

    public WorkflowStep(
        Guid id,
        Guid workflowDefinitionId,
        int stepNumber,
        string name,
        string? description,
        StepType stepType,
        string? handlerType,
        string? configuration,
        int timeout,
        int retryCount,
        int retryDelaySeconds,
        bool isRequired)
        : base(id)
    {
        WorkflowDefinitionId = workflowDefinitionId;
        StepNumber = stepNumber;
        Name = name;
        Description = description;
        StepType = stepType;
        HandlerType = handlerType;
        Configuration = configuration;
        Timeout = timeout;
        RetryCount = retryCount;
        RetryDelaySeconds = retryDelaySeconds;
        IsRequired = isRequired;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid WorkflowDefinitionId { get; private set; }
    public int StepNumber { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public StepType StepType { get; private set; }
    public string? HandlerType { get; private set; }
    public string? Configuration { get; private set; }
    public int Timeout { get; private set; }
    public int RetryCount { get; private set; }
    public int RetryDelaySeconds { get; private set; }
    public bool IsRequired { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public WorkflowDefinition WorkflowDefinition { get; private set; } = null!;
}
