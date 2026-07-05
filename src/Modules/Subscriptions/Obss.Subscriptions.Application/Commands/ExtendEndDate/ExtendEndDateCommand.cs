using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Commands.ExtendEndDate;

public sealed record ExtendEndDateCommand(
    Guid SubscriptionId,
    DateTime NewEndDate) : IRequest<Result<SubscriptionDto>>;
