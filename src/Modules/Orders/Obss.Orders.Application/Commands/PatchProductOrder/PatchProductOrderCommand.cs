using MediatR;
using Obss.Orders.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.PatchProductOrder;

public sealed record PatchProductOrderCommand(
    Guid Id,
    string? Description,
    string? Channel,
    string? Priority,
    string? Notes,
    DateTime? RequestedStartDate,
    DateTime? RequestedCompletionDate,
    DateTime? ExpectedCompletionDate,
    string? NotificationContact,
    string? ExternalId,
    Guid? QuoteId,
    string? BillingAddressStreet,
    string? BillingAddressCity,
    string? BillingAddressState,
    string? BillingAddressPostalCode,
    string? BillingAddressCountry,
    string? ShippingAddressStreet,
    string? ShippingAddressCity,
    string? ShippingAddressState,
    string? ShippingAddressPostalCode,
    string? ShippingAddressCountry) : IRequest<Result<ProductOrderDto>>;
