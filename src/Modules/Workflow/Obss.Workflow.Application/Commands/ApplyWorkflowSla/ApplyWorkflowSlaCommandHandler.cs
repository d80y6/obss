using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Application.DTOs;
using Obss.Workflow.Domain.Entities;

namespace Obss.Workflow.Application.Commands.ApplyWorkflowSla;

public sealed class ApplyWorkflowSlaCommandHandler : IRequestHandler<ApplyWorkflowSlaCommand, Result<WorkflowInstanceDto>>
{
    private readonly IWorkflowInstanceRepository _instanceRepository;
    private readonly IWorkflowSlaRepository _slaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApplyWorkflowSlaCommandHandler(
        IWorkflowInstanceRepository instanceRepository,
        IWorkflowSlaRepository slaRepository,
        IUnitOfWork unitOfWork)
    {
        _instanceRepository = instanceRepository;
        _slaRepository = slaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<WorkflowInstanceDto>> Handle(ApplyWorkflowSlaCommand request, CancellationToken cancellationToken)
    {
        var instance = await _instanceRepository.GetByIdAsync(request.WorkflowInstanceId, cancellationToken);
        if (instance is null)
            return Result.Failure<WorkflowInstanceDto>(Error.NotFound(nameof(WorkflowInstance), request.WorkflowInstanceId));

        var sla = await _slaRepository.GetByIdAsync(request.WorkflowSlaId, cancellationToken);
        if (sla is null)
            return Result.Failure<WorkflowInstanceDto>(Error.NotFound(nameof(WorkflowSla), request.WorkflowSlaId));

        instance.ApplySlaDefinition(sla);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(instance.Adapt<WorkflowInstanceDto>());
    }
}
