using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Commands.ChangeOffer;

public sealed record ChangeOfferCommand(
    Guid SubscriptionId,
    Guid NewOfferId,
    decimal NewPrice) : IRequest<Result<SubscriptionDto>>;
