using Obss.Payments.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Payments.Application.Abstractions;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByIdWithDetailsAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetFilteredAsync(
        Guid? customerId,
        string? status,
        DateTime? fromDate,
        DateTime? toDate,
        int offset = 0,
        int limit = 20,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetByInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Refund>> GetRefundsFilteredAsync(Guid? paymentId, string? status, DateTime? fromDate, DateTime? toDate, int offset = 0, int limit = 20, CancellationToken cancellationToken = default);
    Task<PaymentSummary> GetPaymentSummaryAsync(CancellationToken cancellationToken = default);
    Task<string> GenerateNextPaymentNumberAsync(CancellationToken cancellationToken = default);
}

public sealed record PaymentSummary(
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
