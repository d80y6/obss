namespace Obss.Billing.Application.DTOs;

public sealed record AccountBalanceDto(
    Guid Id,
    Guid BillingAccountId,
    decimal CurrentBalance,
    decimal OutstandingBalance,
    decimal AvailableCredit,
    string Currency,
    DateTime BalanceDate,
    DateTime LastUpdatedAt,
    string? AtType,
    string? AtBaseType,
    string? AtSchemaLocation,
    List<BalanceTransactionDto>? Transactions);
