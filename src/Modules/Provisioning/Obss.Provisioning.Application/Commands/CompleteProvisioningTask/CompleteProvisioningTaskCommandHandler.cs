using Mapster;
using MediatR;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Commands.CompleteProvisioningTask;

public sealed class CompleteProvisioningTaskCommandHandler : IRequestHandler<CompleteProvisioningTaskCommand, Result<ProvisioningTaskDto>>
{
    private readonly IProvisioningJobRepository _jobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteProvisioningTaskCommandHandler(IProvisioningJobRepository jobRepository, IUnitOfWork unitOfWork)
    {
        _jobRepository = jobRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProvisioningTaskDto>> Handle(CompleteProvisioningTaskCommand request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdWithTasksAsync(request.JobId, cancellationToken);

        if (job is null)
            return Result.Failure<ProvisioningTaskDto>(Error.NotFound("ProvisioningJob", request.JobId));

        var task = job.Tasks.FirstOrDefault(t => t.Id == request.TaskId);

        if (task is null)
            return Result.Failure<ProvisioningTaskDto>(Error.NotFound("ProvisioningTask", request.TaskId));

        task.Complete(request.Result);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(task.Adapt<ProvisioningTaskDto>());
    }
}
