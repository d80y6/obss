namespace Obss.Provisioning.Application.DTOs;

public sealed record ProvisioningJobDto(
    Guid Id,
    Guid TenantId,
    Guid OrderId,
    Guid OrderItemId,
    Guid CustomerId,
    Guid? ServiceId,
    string ServiceType,
    string Action,
    string Status,
    Guid? WorkflowInstanceId,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    string? ErrorMessage,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<ProvisioningTaskDto> Tasks);
