using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.DTOs;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Application.Commands.CreateSubscription;

public sealed record CreateSubscriptionCommand(
    string TenantId,
    Guid CustomerId,
    string CustomerName,
    Guid OrderId,
    Guid OrderItemId,
    Guid ProductId,
    Guid OfferId,
    string OfferName,
    BillingPeriod BillingPeriod,
    string Currency,
    decimal Price,
    int Quantity,
    DateTime StartDate,
    DateTime? EndDate) : IRequest<Result<SubscriptionDto>>;
