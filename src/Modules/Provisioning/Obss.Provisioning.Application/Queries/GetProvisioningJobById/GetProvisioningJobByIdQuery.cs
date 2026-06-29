using MediatR;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Queries.GetProvisioningJobById;

public sealed record GetProvisioningJobByIdQuery(Guid JobId) : IRequest<Result<ProvisioningJobDto>>;
