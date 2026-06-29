using System.Text.Json;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Provisioning.Domain.Entities;

public class ProvisioningTask : Entity<Guid>
{
    private ProvisioningTask() { }

    private ProvisioningTask(
        Guid id,
        Guid provisioningJobId,
        int stepNumber,
        ProvisioningTaskType taskType,
        string? assignedTo,
        JsonDocument? configuration)
        : base(id)
    {
        ProvisioningJobId = provisioningJobId;
        StepNumber = stepNumber;
        TaskType = taskType;
        Status = ProvisioningTaskStatus.Pending;
        AssignedTo = assignedTo;
        Configuration = configuration;
    }

    public Guid ProvisioningJobId { get; private set; }
    public int StepNumber { get; private set; }
    public ProvisioningTaskType TaskType { get; private set; }
    public ProvisioningTaskStatus Status { get; private set; }
    public string? AssignedTo { get; private set; }
    public JsonDocument? Configuration { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public JsonDocument? Result { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int RetryCount { get; private set; }

    public static ProvisioningTask Create(
        Guid provisioningJobId,
        int stepNumber,
        ProvisioningTaskType taskType,
        string? assignedTo = null,
        JsonDocument? configuration = null)
    {
        return new ProvisioningTask(
            Guid.NewGuid(),
            provisioningJobId,
            stepNumber,
            taskType,
            assignedTo,
            configuration);
    }

    public void Start()
    {
        if (Status != ProvisioningTaskStatus.Pending)
            return;

        Status = ProvisioningTaskStatus.InProgress;
        StartedAt = DateTime.UtcNow;
    }

    public void Complete(JsonDocument? result = null)
    {
        if (Status != ProvisioningTaskStatus.InProgress)
            return;

        Status = ProvisioningTaskStatus.Completed;
        Result = result;
        CompletedAt = DateTime.UtcNow;
    }

    public void Fail(string error)
    {
        if (Status is ProvisioningTaskStatus.Completed or ProvisioningTaskStatus.Skipped)
            return;

        Status = ProvisioningTaskStatus.Failed;
        ErrorMessage = error;
        CompletedAt = DateTime.UtcNow;
        RetryCount++;
    }

    public void Skip()
    {
        if (Status != ProvisioningTaskStatus.Pending)
            return;

        Status = ProvisioningTaskStatus.Skipped;
        CompletedAt = DateTime.UtcNow;
    }
}
