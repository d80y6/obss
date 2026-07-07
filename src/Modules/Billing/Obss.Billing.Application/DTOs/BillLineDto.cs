namespace Obss.Billing.Application.DTOs;

public sealed record BillLineDto(
    Guid Id,
    Guid BillId,
    string LineType,
    string Description,
    Guid? SubscriptionId,
    Guid? ProductId,
    Guid? OfferId,
    int Quantity,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TaxRate,
    decimal TotalAmount,
    string Currency,
    DateTime LineDate,
    string? Reference,
    string? ExternalId);
