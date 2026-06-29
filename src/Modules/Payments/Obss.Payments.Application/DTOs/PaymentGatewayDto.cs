namespace Obss.Payments.Application.DTOs;

public sealed record PaymentGatewayDto(
    Guid Id,
    string TenantId,
    string Name,
    string Provider,
    bool IsActive,
    string Configuration,
    List<string> SupportedCurrencies,
    decimal? MinAmount,
    decimal? MaxAmount,
    decimal TransactionFee,
    string FeeType,
    DateTime CreatedAt);

public sealed record RegisterPaymentGatewayRequest(
    string Name,
    string Provider,
    string Configuration,
    List<string> SupportedCurrencies,
    decimal? MinAmount,
    decimal? MaxAmount,
    decimal TransactionFee,
    string FeeType);

public sealed record ProcessPaymentRequestDto(
    decimal Amount,
    string Currency,
    string PaymentMethod,
    string? ReturnUrl,
    string? CancelUrl,
    Guid CustomerId,
    string? Description);
