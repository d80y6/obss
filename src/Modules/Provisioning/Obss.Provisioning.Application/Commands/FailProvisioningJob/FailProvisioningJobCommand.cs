using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Commands.FailProvisioningJob;

public sealed record FailProvisioningJobCommand(Guid JobId, string ErrorMessage) : IRequest<Result>;
