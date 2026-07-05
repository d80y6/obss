using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Commands.ChangeQuantity;

public sealed record ChangeQuantityCommand(
    Guid SubscriptionId,
    int NewQuantity) : IRequest<Result<SubscriptionDto>>;
