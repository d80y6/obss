namespace Obss.Billing.Application.DTOs;

public sealed record BillingAccountDto(
    Guid Id,
    Guid CustomerId,
    string AccountType,
    string Name,
    string Status,
    decimal CreditLimit,
    string Currency,
    DateTime? ValidFrom,
    DateTime? ValidUntil,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
