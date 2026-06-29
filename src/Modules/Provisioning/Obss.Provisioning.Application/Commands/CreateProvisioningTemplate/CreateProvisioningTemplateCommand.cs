using MediatR;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Commands.CreateProvisioningTemplate;

public sealed record CreateProvisioningTemplateCommand(
    Guid TenantId,
    string Name,
    string? Description,
    string ServiceType,
    string Action,
    Guid WorkflowDefinitionId) : IRequest<Result<ProvisioningTemplateDto>>;
