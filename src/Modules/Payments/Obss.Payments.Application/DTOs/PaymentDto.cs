namespace Obss.Payments.Application.DTOs;

public sealed record RelatedPartyDto(string PartyId, string PartyName, string Role);

public sealed record PaymentDto(
    Guid Id,
    string TenantId,
    string PaymentNumber,
    Guid CustomerId,
    Guid? InvoiceId,
    decimal Amount,
    string Currency,
    string PaymentMethod,
    string PaymentReference,
    string Status,
    DateTime PaidAt,
    DateTime? CompletedAt,
    string? Notes,
    string? Href,
    string? AtType,
    string? AtBaseType,
    string? AtSchemaLocation,
    string? ExternalId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<PaymentAllocationDto> Allocations,
    List<RefundDto> Refunds,
    List<RelatedPartyDto>? RelatedParties);

public sealed record PaymentAllocationDto(
    Guid Id,
    Guid InvoiceId,
    decimal Amount,
    string? ExternalId,
    DateTime CreatedAt);


