using Obss.Payments.Domain.ValueObjects;

namespace Obss.Payments.Domain.Services;

public sealed record PaymentResult(
    bool Success,
    string TransactionId,
    PaymentStatus Status,
    string? Message,
    string? RedirectUrl);

public sealed record RefundResult(
    bool Success,
    string TransactionId,
    PaymentStatus Status,
    string? Message);
