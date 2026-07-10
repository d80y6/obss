namespace Obss.Orders.Application.DTOs;

public sealed record ProductOrderItemPriceDto(
    decimal UnitPrice,
    decimal RecurringPrice,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TotalPrice);
