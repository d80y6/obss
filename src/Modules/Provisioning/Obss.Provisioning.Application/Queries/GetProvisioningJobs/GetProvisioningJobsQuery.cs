using MediatR;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Queries.GetProvisioningJobs;

public sealed record GetProvisioningJobsQuery(
    Guid? OrderId,
    string? Status,
    Guid? ServiceId,
    int Offset = 0,
    int Limit = 20) : IRequest<Result<IReadOnlyList<ProvisioningJobDto>>>;
