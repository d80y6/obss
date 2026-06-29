using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Application.DTOs;
using Obss.Workflow.Domain.Entities;

namespace Obss.Workflow.Application.Commands.RemoveWorkflowStep;

public sealed class RemoveWorkflowStepCommandHandler : IRequestHandler<RemoveWorkflowStepCommand, Result<WorkflowDefinitionDto>>
{
    private readonly IWorkflowDefinitionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveWorkflowStepCommandHandler(IWorkflowDefinitionRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<WorkflowDefinitionDto>> Handle(RemoveWorkflowStepCommand request, CancellationToken cancellationToken)
    {
        var definition = await _repository.GetByIdAsync(request.WorkflowDefinitionId, cancellationToken);

        if (definition is null)
            return Result.Failure<WorkflowDefinitionDto>(Error.NotFound(nameof(WorkflowDefinition), request.WorkflowDefinitionId));

        definition.RemoveStep(request.StepId);

        await _repository.UpdateAsync(definition, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(definition.Adapt<WorkflowDefinitionDto>());
    }
}
