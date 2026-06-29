using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Application.DTOs;
using Obss.Workflow.Domain.Entities;

namespace Obss.Workflow.Application.Queries.GetWorkflowDefinitionById;

public sealed class GetWorkflowDefinitionByIdQueryHandler : IRequestHandler<GetWorkflowDefinitionByIdQuery, Result<WorkflowDefinitionDto>>
{
    private readonly IWorkflowDefinitionRepository _repository;

    public GetWorkflowDefinitionByIdQueryHandler(IWorkflowDefinitionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<WorkflowDefinitionDto>> Handle(GetWorkflowDefinitionByIdQuery request, CancellationToken cancellationToken)
    {
        var definition = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (definition is null)
            return Result.Failure<WorkflowDefinitionDto>(Error.NotFound(nameof(WorkflowDefinition), request.Id));

        return Result.Success(definition.Adapt<WorkflowDefinitionDto>());
    }
}
