using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.AddProductOrderItem;

public sealed record AddProductOrderItemCommand(
    Guid OrderId,
    Guid ProductId,
    Guid OfferId,
    string ProductName,
    string OfferName,
    int Quantity,
    decimal UnitPrice,
    decimal RecurringPrice,
    decimal DiscountAmount,
    decimal TaxAmount,
    string BillingPeriod,
    DateTime? StartDate,
    DateTime? EndDate) : IRequest<Result>;
