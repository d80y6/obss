using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.DTOs;

namespace Obss.Workflow.Application.Queries.GetWorkflowDefinitions;

public sealed record GetWorkflowDefinitionsQuery(
    string? TenantId,
    string? Category,
    bool? IsActive,
    string? SearchTerm,
    int Offset = 0,
    int Limit = 20) : IRequest<Result<IReadOnlyList<WorkflowDefinitionDto>>>;
