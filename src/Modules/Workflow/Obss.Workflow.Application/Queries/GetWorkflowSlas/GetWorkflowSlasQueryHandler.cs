using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Application.DTOs;
using Obss.Workflow.Domain.Entities;

namespace Obss.Workflow.Application.Queries.GetWorkflowSlas;

public sealed class GetWorkflowSlasQueryHandler : IRequestHandler<GetWorkflowSlasQuery, Result<IReadOnlyList<WorkflowSlaDto>>>
{
    private readonly IWorkflowSlaRepository _repository;

    public GetWorkflowSlasQueryHandler(IWorkflowSlaRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<WorkflowSlaDto>>> Handle(GetWorkflowSlasQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<WorkflowSla> slas;

        if (request.WorkflowDefinitionId.HasValue)
        {
            slas = await _repository.GetByWorkflowDefinitionIdAsync(request.WorkflowDefinitionId.Value, cancellationToken);
        }
        else
        {
            slas = await _repository.GetAllAsync(cancellationToken);
        }

        var result = slas.Adapt<List<WorkflowSlaDto>>();
        return Result.Success<IReadOnlyList<WorkflowSlaDto>>(result);
    }
}
