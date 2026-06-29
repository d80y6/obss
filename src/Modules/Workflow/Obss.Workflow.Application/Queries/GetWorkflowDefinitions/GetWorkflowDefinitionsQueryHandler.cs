using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Application.DTOs;

namespace Obss.Workflow.Application.Queries.GetWorkflowDefinitions;

public sealed class GetWorkflowDefinitionsQueryHandler : IRequestHandler<GetWorkflowDefinitionsQuery, Result<IReadOnlyList<WorkflowDefinitionDto>>>
{
    private readonly IWorkflowDefinitionRepository _repository;

    public GetWorkflowDefinitionsQueryHandler(IWorkflowDefinitionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<WorkflowDefinitionDto>>> Handle(GetWorkflowDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var definitions = await _repository.GetFilteredAsync(
            request.TenantId,
            request.Category,
            request.IsActive,
            request.SearchTerm,
            request.Page,
            request.PageSize,
            cancellationToken);

        var result = definitions.Adapt<List<WorkflowDefinitionDto>>();
        return Result.Success<IReadOnlyList<WorkflowDefinitionDto>>(result);
    }
}
