using MediatR;
using Obss.Orders.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.CreateOrder;

public sealed record CreateOrderItemRequest(
    Guid ProductId,
    Guid OfferId,
    string ProductName,
    string OfferName,
    int Quantity,
    decimal UnitPrice,
    decimal RecurringPrice,
    decimal DiscountAmount,
    decimal TaxAmount,
    string BillingPeriod);

public sealed record CreateOrderCommand(
    Guid CustomerId,
    string CustomerName,
    string OrderType,
    string? Notes,
    string? BillingAddressStreet,
    string? BillingAddressCity,
    string? BillingAddressState,
    string? BillingAddressPostalCode,
    string? BillingAddressCountry,
    string? ShippingAddressStreet,
    string? ShippingAddressCity,
    string? ShippingAddressState,
    string? ShippingAddressPostalCode,
    string? ShippingAddressCountry,
    string Currency,
    List<CreateOrderItemRequest> Items) : IRequest<Result<OrderDto>>;
