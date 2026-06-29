using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Commands.RetryProvisioningJob;

public sealed record RetryProvisioningJobCommand(Guid JobId) : IRequest<Result>;
