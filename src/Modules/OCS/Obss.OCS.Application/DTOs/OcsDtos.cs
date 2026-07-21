namespace Obss.OCS.Application.DTOs;

public sealed record BalanceDto(
    Guid Id,
    Guid SubscriptionId,
    decimal AvailableAmount,
    decimal ReservedAmount,
    decimal EffectiveBalance,
    string Currency);

public sealed record CreditPoolDto(
    Guid Id,
    string Name,
    decimal TotalAmount,
    decimal RemainingAmount,
    string Currency,
    string Status,
    DateTime? ExpiresAt);

public sealed record OcsTransactionDto(
    Guid Id,
    Guid SubscriptionId,
    string TransactionType,
    decimal Amount,
    string Currency,
    string Description,
    DateTime Timestamp);
