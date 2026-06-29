using MediatR;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Commands.StartProvisioningJob;

public sealed record StartProvisioningJobCommand(Guid JobId) : IRequest<Result<ProvisioningJobDto>>;
