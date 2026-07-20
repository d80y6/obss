using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Services;

public sealed record ReconciliationResult(
    bool Success,
    string TransactionId,
    string InvoiceId,
    decimal AmountApplied,
    decimal RemainingAmount,
    string Status);

public sealed record ReconciliationReport(
    DateTime From,
    DateTime To,
    int TotalPayments,
    int MatchedPayments,
    int PartialPayments,
    int Overpayments,
    int UnmatchedPayments,
    decimal TotalAmount,
    decimal MatchedAmount,
    decimal UnmatchedAmount,
    IReadOnlyCollection<ReconciliationResult> Details);

public interface IPaymentReconciliationService
{
    Task<Result<ReconciliationResult>> MatchPaymentToInvoiceAsync(
        Guid paymentId,
        Guid invoiceId,
        CancellationToken cancellationToken = default);

    Task<Result<ReconciliationResult>> HandlePartialPaymentAsync(
        Guid paymentId,
        Guid invoiceId,
        decimal amountApplied,
        CancellationToken cancellationToken = default);

    Task<Result<ReconciliationResult>> HandleOverpaymentAsync(
        Guid paymentId,
        Guid invoiceId,
        CancellationToken cancellationToken = default);

    Task<ReconciliationReport> GenerateReconciliationReportAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
}
