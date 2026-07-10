using MediatR;
using Obss.Orders.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.CreateProductOrder;

public sealed record CreateProductOrderItemRequest(
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

public sealed record CreateProductOrderCommand(
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
    Guid? BillingAccountId,
    string? Priority,
    Guid? ProductOfferingQualificationId,
    string? QuoteHref,
    List<CreateProductOrderItemRequest> Items) : IRequest<Result<ProductOrderDto>>;
