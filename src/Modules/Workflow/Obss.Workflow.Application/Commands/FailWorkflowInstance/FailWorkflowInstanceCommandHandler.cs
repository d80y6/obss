using MediatR;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Domain.Entities;

namespace Obss.Workflow.Application.Commands.FailWorkflowInstance;

public sealed class FailWorkflowInstanceCommandHandler : IRequestHandler<FailWorkflowInstanceCommand, Result>
{
    private readonly IWorkflowInstanceRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public FailWorkflowInstanceCommandHandler(IWorkflowInstanceRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(FailWorkflowInstanceCommand request, CancellationToken cancellationToken)
    {
        var instance = await _repository.GetByIdAsync(request.InstanceId, cancellationToken);

        if (instance is null)
            return Result.Failure(Error.NotFound(nameof(WorkflowInstance), request.InstanceId));

        instance.Fail(request.Error);
        await _repository.UpdateAsync(instance, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
