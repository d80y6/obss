namespace Obss.Provisioning.Application.DTOs;

public sealed record ProvisioningTemplateDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string? Description,
    string ServiceType,
    string Action,
    Guid WorkflowDefinitionId,
    bool IsActive,
    DateTime CreatedAt);
