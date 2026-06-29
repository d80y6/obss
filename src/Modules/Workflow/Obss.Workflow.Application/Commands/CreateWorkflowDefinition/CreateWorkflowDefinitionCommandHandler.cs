using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Application.DTOs;
using Obss.Workflow.Domain.Entities;
using Obss.Workflow.Domain.ValueObjects;

namespace Obss.Workflow.Application.Commands.CreateWorkflowDefinition;

public sealed class CreateWorkflowDefinitionCommandHandler : IRequestHandler<CreateWorkflowDefinitionCommand, Result<WorkflowDefinitionDto>>
{
    private readonly IWorkflowDefinitionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWorkflowDefinitionCommandHandler(IWorkflowDefinitionRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<WorkflowDefinitionDto>> Handle(CreateWorkflowDefinitionCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<WorkflowCategory>(request.Category, true, out var category))
            return Result.Failure<WorkflowDefinitionDto>(Error.Validation($"Invalid category '{request.Category}'."));

        var definition = WorkflowDefinition.Create(
            request.TenantId,
            request.Name,
            request.Description,
            category);

        await _repository.AddAsync(definition, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(definition.Adapt<WorkflowDefinitionDto>());
    }
}
