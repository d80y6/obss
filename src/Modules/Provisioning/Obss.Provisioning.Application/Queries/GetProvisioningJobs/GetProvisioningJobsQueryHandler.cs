using Mapster;
using MediatR;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Queries.GetProvisioningJobs;

public sealed class GetProvisioningJobsQueryHandler : IRequestHandler<GetProvisioningJobsQuery, Result<IReadOnlyList<ProvisioningJobDto>>>
{
    private readonly IProvisioningJobRepository _jobRepository;

    public GetProvisioningJobsQueryHandler(IProvisioningJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<Result<IReadOnlyList<ProvisioningJobDto>>> Handle(GetProvisioningJobsQuery request, CancellationToken cancellationToken)
    {
        var jobs = await _jobRepository.GetFilteredAsync(
            request.OrderId,
            request.Status,
            request.ServiceId,
            request.Offset,
            request.Limit,
            cancellationToken);

        return Result.Success(jobs.Adapt<IReadOnlyList<ProvisioningJobDto>>());
    }
}
