using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Application.DTOs;
using Obss.Workflow.Domain.Entities;
using Obss.Workflow.Domain.Services;

namespace Obss.Workflow.Application.Commands.StartWorkflowInstance;

public sealed class StartWorkflowInstanceCommandHandler : IRequestHandler<StartWorkflowInstanceCommand, Result<WorkflowInstanceDto>>
{
    private readonly IWorkflowEngine _workflowEngine;
    private readonly IWorkflowDefinitionRepository _definitionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StartWorkflowInstanceCommandHandler(
        IWorkflowEngine workflowEngine,
        IWorkflowDefinitionRepository definitionRepository,
        IUnitOfWork unitOfWork)
    {
        _workflowEngine = workflowEngine;
        _definitionRepository = definitionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<WorkflowInstanceDto>> Handle(StartWorkflowInstanceCommand request, CancellationToken cancellationToken)
    {
        var definition = await _definitionRepository.GetByIdAsync(request.WorkflowDefinitionId, cancellationToken);

        if (definition is null)
            return Result.Failure<WorkflowInstanceDto>(Error.NotFound(nameof(WorkflowDefinition), request.WorkflowDefinitionId));

        if (!definition.IsActive)
            return Result.Failure<WorkflowInstanceDto>(Error.Validation("Cannot start an inactive workflow definition."));

        var instance = await _workflowEngine.StartWorkflow(
            request.WorkflowDefinitionId,
            request.TriggerEntityType,
            request.TriggerEntityId,
            request.CreatedBy,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(instance.Adapt<WorkflowInstanceDto>());
    }
}
