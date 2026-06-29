using Mapster;
using MediatR;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Queries.GetProvisioningJobById;

public sealed class GetProvisioningJobByIdQueryHandler : IRequestHandler<GetProvisioningJobByIdQuery, Result<ProvisioningJobDto>>
{
    private readonly IProvisioningJobRepository _jobRepository;

    public GetProvisioningJobByIdQueryHandler(IProvisioningJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<Result<ProvisioningJobDto>> Handle(GetProvisioningJobByIdQuery request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdWithTasksAsync(request.JobId, cancellationToken);

        if (job is null)
            return Result.Failure<ProvisioningJobDto>(Error.NotFound("ProvisioningJob", request.JobId));

        return Result.Success(job.Adapt<ProvisioningJobDto>());
    }
}
