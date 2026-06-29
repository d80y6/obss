using MediatR;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Queries.GetProvisioningJobs;

public sealed record GetProvisioningJobsQuery(
    Guid? OrderId,
    string? Status,
    Guid? ServiceId,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<IReadOnlyList<ProvisioningJobDto>>>;
