using System.Text.Json;
using MediatR;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Commands.CompleteProvisioningTask;

public sealed record CompleteProvisioningTaskCommand(
    Guid JobId,
    Guid TaskId,
    JsonDocument? Result) : IRequest<Result<ProvisioningTaskDto>>;
