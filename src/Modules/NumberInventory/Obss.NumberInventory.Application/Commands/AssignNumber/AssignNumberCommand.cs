using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Commands.AssignNumber;

public sealed record AssignNumberCommand(
    Guid NumberId,
    Guid CustomerId,
    Guid SubscriptionId) : IRequest<Result>;
