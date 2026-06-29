using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Commands.CheckCapacityThreshold;

public sealed record CheckCapacityThresholdCommand(
    Guid ElementId,
    decimal Threshold) : IRequest<Result<bool>>;
