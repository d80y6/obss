using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Queries.GetSubscriptionById;

public sealed record GetSubscriptionByIdQuery(Guid Id) : IRequest<Result<SubscriptionDto>>;
