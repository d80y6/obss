using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Application.DTOs;
using Obss.Workflow.Domain.Entities;

namespace Obss.Workflow.Application.Commands.CreateWorkflowSla;

public sealed class CreateWorkflowSlaCommandHandler : IRequestHandler<CreateWorkflowSlaCommand, Result<WorkflowSlaDto>>
{
    private readonly IWorkflowSlaRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWorkflowSlaCommandHandler(IWorkflowSlaRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<WorkflowSlaDto>> Handle(CreateWorkflowSlaCommand request, CancellationToken cancellationToken)
    {
        var sla = WorkflowSla.Create(
            request.TenantId,
            request.Name,
            request.Description,
            request.WorkflowDefinitionId,
            request.TargetDurationMinutes,
            request.WarningThresholdPercent,
            request.EscalationUserId,
            request.EscalationGroup);

        await _repository.AddAsync(sla, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(sla.Adapt<WorkflowSlaDto>());
    }
}
