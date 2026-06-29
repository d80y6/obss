namespace Obss.Billing.Application.DTOs;

public sealed record BillDto(
    Guid Id,
    string TenantId,
    Guid CustomerId,
    string CustomerName,
    string BillingPeriod,
    DateTime BillingPeriodStart,
    DateTime BillingPeriodEnd,
    DateTime DueDate,
    string Status,
    decimal SubTotal,
    decimal DiscountTotal,
    decimal TaxTotal,
    decimal GrandTotal,
    string Currency,
    DateTime CreatedAt,
    DateTime? FinalizedAt,
    List<BillLineDto> Lines);
