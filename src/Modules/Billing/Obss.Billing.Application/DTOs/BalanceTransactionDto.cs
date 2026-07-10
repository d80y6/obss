namespace Obss.Billing.Application.DTOs;

public sealed record BalanceTransactionDto(
    Guid Id,
    decimal Amount,
    string TransactionType,
    string Description,
    DateTime TransactionDate,
    string? ReferenceId,
    string? ReferenceType);
