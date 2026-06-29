using MediatR;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Commands.CreateProvisioningJob;

public sealed record CreateProvisioningJobCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid CustomerId,
    Guid TenantId,
    string ServiceType,
    string Action) : IRequest<Result<ProvisioningJobDto>>;
