using MediatR;
using Obss.Provisioning.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Commands.FailProvisioningJob;

public sealed class FailProvisioningJobCommandHandler : IRequestHandler<FailProvisioningJobCommand, Result>
{
    private readonly IProvisioningJobRepository _jobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FailProvisioningJobCommandHandler(IProvisioningJobRepository jobRepository, IUnitOfWork unitOfWork)
    {
        _jobRepository = jobRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(FailProvisioningJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken);

        if (job is null)
            return Result.Failure(Error.NotFound("ProvisioningJob", request.JobId));

        job.Fail(request.ErrorMessage);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
