namespace Obss.Payments.Application.DTOs;

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
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<PaymentAllocationDto> Allocations,
    List<RefundDto> Refunds);

public sealed record PaymentAllocationDto(
    Guid Id,
    Guid InvoiceId,
    decimal Amount,
    DateTime CreatedAt);


