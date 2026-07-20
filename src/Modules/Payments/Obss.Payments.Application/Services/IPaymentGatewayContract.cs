using Obss.Payments.Domain.ValueObjects;

namespace Obss.Payments.Application.Services;

public sealed record PaymentContractResult(
    bool Success,
    string TransactionId,
    PaymentStatus Status,
    string? Message,
    string? ConfirmationCode,
    DateTime ProcessedAt);

public sealed record PendingReconciliationItem(
    string TransactionId,
    PaymentProvider Provider,
    decimal Amount,
    string Currency,
    string CustomerId,
    DateTime TransactionDate,
    PaymentStatus Status);

public interface IPaymentGatewayContract
{
    Task<PaymentContractResult> ProcessLocalBankTransferAsync(
        string tenantId,
        string customerId,
        decimal amount,
        string currency,
        string bankReference,
        string bankName,
        CancellationToken cancellationToken = default);

    Task<PaymentContractResult> ProcessMobileMoneyAsync(
        string tenantId,
        string customerId,
        decimal amount,
        string currency,
        string mobileProvider,
        string mobileNumber,
        string transactionReference,
        CancellationToken cancellationToken = default);

    Task<PaymentContractResult> ProcessCashAtAgentAsync(
        string tenantId,
        string customerId,
        decimal amount,
        string currency,
        string agentId,
        string agentName,
        string receiptNumber,
        CancellationToken cancellationToken = default);

    Task<PaymentContractResult> ConfirmPaymentAsync(
        string transactionId,
        string confirmedBy,
        CancellationToken cancellationToken = default);

    Task<PaymentContractResult> DeclinePaymentAsync(
        string transactionId,
        string reason,
        string declinedBy,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PendingReconciliationItem>> GetPendingReconciliationAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
}
