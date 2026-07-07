namespace Obss.Billing.Application.DTOs;

public sealed record RelatedPartyDto(string PartyId, string PartyName, string Role);

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
    string? Href,
    string? AtType,
    string? AtBaseType,
    string? AtSchemaLocation,
    string? ExternalId,
    List<BillLineDto> Lines,
    List<RelatedPartyDto>? RelatedParties);
