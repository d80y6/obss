using MediatR;
using Obss.Provisioning.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Commands.RetryProvisioningJob;

public sealed class RetryProvisioningJobCommandHandler : IRequestHandler<RetryProvisioningJobCommand, Result>
{
    private readonly IProvisioningJobRepository _jobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RetryProvisioningJobCommandHandler(IProvisioningJobRepository jobRepository, IUnitOfWork unitOfWork)
    {
        _jobRepository = jobRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RetryProvisioningJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken);

        if (job is null)
            return Result.Failure(Error.NotFound("ProvisioningJob", request.JobId));

        job.Retry();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
