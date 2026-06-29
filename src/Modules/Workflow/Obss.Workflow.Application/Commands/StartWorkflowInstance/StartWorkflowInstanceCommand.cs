using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.DTOs;

namespace Obss.Workflow.Application.Commands.StartWorkflowInstance;

public sealed record StartWorkflowInstanceCommand(
    Guid WorkflowDefinitionId,
    string TriggerEntityType,
    Guid TriggerEntityId,
    string CreatedBy) : IRequest<Result<WorkflowInstanceDto>>;
