using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Application.DTOs;
using Obss.Workflow.Domain.Services;

namespace Obss.Workflow.Application.Commands.ExecuteWorkflowTask;

public sealed class ExecuteWorkflowTaskCommandHandler : IRequestHandler<ExecuteWorkflowTaskCommand, Result<WorkflowTaskDto>>
{
    private readonly IWorkflowEngine _workflowEngine;
    private readonly IUnitOfWork _unitOfWork;

    public ExecuteWorkflowTaskCommandHandler(IWorkflowEngine workflowEngine, IUnitOfWork unitOfWork)
    {
        _workflowEngine = workflowEngine;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<WorkflowTaskDto>> Handle(ExecuteWorkflowTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _workflowEngine.ExecuteStep(request.InstanceId, request.TaskId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(task.Adapt<WorkflowTaskDto>());
    }
}
