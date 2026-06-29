using MediatR;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Queries.GetServicesBySubscription;

public sealed record GetServicesBySubscriptionQuery(Guid SubscriptionId) : IRequest<Result<IReadOnlyList<ServiceDto>>>;
