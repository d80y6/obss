using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Application.DTOs;
using Obss.Workflow.Domain.Entities;

namespace Obss.Workflow.Application.Queries.GetWorkflowInstanceById;

public sealed class GetWorkflowInstanceByIdQueryHandler : IRequestHandler<GetWorkflowInstanceByIdQuery, Result<WorkflowInstanceDto>>
{
    private readonly IWorkflowInstanceRepository _repository;

    public GetWorkflowInstanceByIdQueryHandler(IWorkflowInstanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<WorkflowInstanceDto>> Handle(GetWorkflowInstanceByIdQuery request, CancellationToken cancellationToken)
    {
        var instance = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (instance is null)
            return Result.Failure<WorkflowInstanceDto>(Error.NotFound(nameof(WorkflowInstance), request.Id));

        return Result.Success(instance.Adapt<WorkflowInstanceDto>());
    }
}
