using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Application.DTOs;
using Obss.Workflow.Domain.Entities;
using Obss.Workflow.Domain.ValueObjects;

namespace Obss.Workflow.Application.Commands.AddWorkflowStep;

public sealed class AddWorkflowStepCommandHandler : IRequestHandler<AddWorkflowStepCommand, Result<WorkflowDefinitionDto>>
{
    private readonly IWorkflowDefinitionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AddWorkflowStepCommandHandler(IWorkflowDefinitionRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<WorkflowDefinitionDto>> Handle(AddWorkflowStepCommand request, CancellationToken cancellationToken)
    {
        var definition = await _repository.GetByIdAsync(request.WorkflowDefinitionId, cancellationToken);

        if (definition is null)
            return Result.Failure<WorkflowDefinitionDto>(Error.NotFound(nameof(WorkflowDefinition), request.WorkflowDefinitionId));

        if (!Enum.TryParse<StepType>(request.StepType, true, out var stepType))
            return Result.Failure<WorkflowDefinitionDto>(Error.Validation($"Invalid step type '{request.StepType}'."));

        definition.AddStep(
            request.StepNumber,
            request.Name,
            request.Description,
            stepType,
            request.HandlerType,
            request.Configuration,
            request.Timeout,
            request.RetryCount,
            request.RetryDelaySeconds,
            request.IsRequired);

        await _repository.UpdateAsync(definition, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(definition.Adapt<WorkflowDefinitionDto>());
    }
}
