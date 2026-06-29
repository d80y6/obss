namespace Obss.Payments.Application.DTOs;

public sealed record PaymentSummaryDto(
    int TotalPayments,
    int PendingCount,
    int CompletedCount,
    int FailedCount,
    int RefundedCount,
    int PartiallyRefundedCount,
    decimal TotalAmount,
    decimal TotalCompletedAmount,
    decimal TotalRefundedAmount,
    decimal NetAmount);
