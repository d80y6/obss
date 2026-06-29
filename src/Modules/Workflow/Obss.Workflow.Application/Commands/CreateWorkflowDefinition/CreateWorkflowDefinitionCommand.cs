using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.DTOs;

namespace Obss.Workflow.Application.Commands.CreateWorkflowDefinition;

public sealed record CreateWorkflowDefinitionCommand(
    string TenantId,
    string Name,
    string? Description,
    string Category) : IRequest<Result<WorkflowDefinitionDto>>;
