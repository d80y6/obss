using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Queries.GetActiveSubscriptionsByCustomer;

public sealed record GetActiveSubscriptionsByCustomerQuery(Guid CustomerId) : IRequest<Result<IReadOnlyList<SubscriptionSummaryDto>>>;
