namespace Obss.Orders.Application.DTOs;

public sealed record OrderItemDto(
    Guid Id,
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
    decimal TotalPrice,
    string BillingPeriod,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsActive,
    string? ServiceType = null,
    string? Action = null,
    string? ItemState = null);

public sealed record OrderPaymentDto(
    Guid Id,
    Guid OrderId,
    decimal Amount,
    string PaymentMethod,
    string PaymentReference,
    DateTime PaidAt,
    string Status);
