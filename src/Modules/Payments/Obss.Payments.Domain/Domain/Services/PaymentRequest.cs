namespace Obss.Payments.Domain.Services;

public sealed record PaymentRequest(
    decimal Amount,
    string Currency,
    string PaymentMethod,
    string? ReturnUrl,
    string? CancelUrl,
    string CustomerId,
    string? Description);
