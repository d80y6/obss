namespace Obss.Provisioning.Application.DTOs;

public sealed record ProvisioningTaskDto(
    Guid Id,
    Guid ProvisioningJobId,
    int StepNumber,
    string TaskType,
    string Status,
    string? AssignedTo,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    string? ErrorMessage,
    int RetryCount);
