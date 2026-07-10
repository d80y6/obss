namespace Obss.Billing.Application.DTOs;

public sealed record PatchBillingAccountRequest(
    string? Name,
    decimal? CreditLimit,
    string? Currency,
    string? Description,
    string? Status,
    string? PaymentMethodId,
    AccountHolderDto? AccountHolder);
