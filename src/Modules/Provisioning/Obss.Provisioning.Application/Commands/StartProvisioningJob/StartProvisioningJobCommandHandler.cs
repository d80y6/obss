using Mapster;
using MediatR;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Commands.StartProvisioningJob;

public sealed class StartProvisioningJobCommandHandler : IRequestHandler<StartProvisioningJobCommand, Result<ProvisioningJobDto>>
{
    private readonly IProvisioningJobRepository _jobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StartProvisioningJobCommandHandler(IProvisioningJobRepository jobRepository, IUnitOfWork unitOfWork)
    {
        _jobRepository = jobRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProvisioningJobDto>> Handle(StartProvisioningJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken);

        if (job is null)
            return Result.Failure<ProvisioningJobDto>(Error.NotFound("ProvisioningJob", request.JobId));

        job.Start();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(job.Adapt<ProvisioningJobDto>());
    }
}
