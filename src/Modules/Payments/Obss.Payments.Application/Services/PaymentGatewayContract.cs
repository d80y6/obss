using Microsoft.Extensions.Logging;
using Obss.Payments.Application.Abstractions;
using Obss.Payments.Domain.Entities;
using Obss.Payments.Domain.ValueObjects;

namespace Obss.Payments.Application.Services;

public sealed class PaymentGatewayContract : IPaymentGatewayContract
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<PaymentGatewayContract> _logger;

    public PaymentGatewayContract(
        IPaymentRepository paymentRepository,
        ILogger<PaymentGatewayContract> logger)
    {
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task<PaymentContractResult> ProcessLocalBankTransferAsync(
        string tenantId,
        string customerId,
        decimal amount,
        string currency,
        string bankReference,
        string bankName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Local bank transfer: customer {CustomerId}, {Amount} {Currency}, bank {BankName}, ref {Reference}",
            customerId, amount, currency, bankName, bankReference);

        var paymentNumber = $"BNK-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..20];
        var payment = Payment.Create(
            tenantId,
            paymentNumber,
            Guid.Parse(customerId),
            amount,
            currency,
            PaymentMethodType.BankTransfer,
            bankReference);

        payment.Complete();

        await _paymentRepository.AddAsync(payment, cancellationToken);

        var transactionId = payment.Id.ToString();
        var confirmationCode = $"BNK-CONF-{Guid.NewGuid():N}"[..16];

        _logger.LogInformation(
            "Local bank transfer completed: {TransactionId}, confirmation {Confirmation}",
            transactionId, confirmationCode);

        return new PaymentContractResult(
            true,
            transactionId,
            PaymentStatus.Completed,
            $"Local bank transfer from {bankName} completed successfully.",
            confirmationCode,
            DateTime.UtcNow);
    }

    public async Task<PaymentContractResult> ProcessMobileMoneyAsync(
        string tenantId,
        string customerId,
        decimal amount,
        string currency,
        string mobileProvider,
        string mobileNumber,
        string transactionReference,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Mobile money: customer {CustomerId}, {Amount} {Currency}, provider {Provider}, number {Number}, ref {Reference}",
            customerId, amount, currency, mobileProvider, mobileNumber, transactionReference);

        var paymentNumber = $"MM-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..20];
        var payment = Payment.Create(
            tenantId,
            paymentNumber,
            Guid.Parse(customerId),
            amount,
            currency,
            PaymentMethodType.MobileMoney,
            transactionReference);

        payment.Complete();

        await _paymentRepository.AddAsync(payment, cancellationToken);

        var transactionId = payment.Id.ToString();
        var confirmationCode = $"MM-CONF-{Guid.NewGuid():N}"[..16];

        _logger.LogInformation(
            "Mobile money payment completed: {TransactionId}, confirmation {Confirmation}",
            transactionId, confirmationCode);

        return new PaymentContractResult(
            true,
            transactionId,
            PaymentStatus.Completed,
            $"Mobile money payment via {mobileProvider} completed successfully.",
            confirmationCode,
            DateTime.UtcNow);
    }

    public async Task<PaymentContractResult> ProcessCashAtAgentAsync(
        string tenantId,
        string customerId,
        decimal amount,
        string currency,
        string agentId,
        string agentName,
        string receiptNumber,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Cash at agent: customer {CustomerId}, {Amount} {Currency}, agent {AgentName} ({AgentId}), receipt {Receipt}",
            customerId, amount, currency, agentName, agentId, receiptNumber);

        var paymentNumber = $"CSH-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..20];
        var payment = Payment.Create(
            tenantId,
            paymentNumber,
            Guid.Parse(customerId),
            amount,
            currency,
            PaymentMethodType.Cash,
            receiptNumber);

        payment.Complete();

        await _paymentRepository.AddAsync(payment, cancellationToken);

        var transactionId = payment.Id.ToString();
        var confirmationCode = $"CSH-CONF-{Guid.NewGuid():N}"[..16];

        _logger.LogInformation(
            "Cash payment completed: {TransactionId}, confirmation {Confirmation}",
            transactionId, confirmationCode);

        return new PaymentContractResult(
            true,
            transactionId,
            PaymentStatus.Completed,
            $"Cash payment at agent {agentName} completed successfully.",
            confirmationCode,
            DateTime.UtcNow);
    }

    public async Task<PaymentContractResult> ConfirmPaymentAsync(
        string transactionId,
        string confirmedBy,
        CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(Guid.Parse(transactionId), cancellationToken);
        if (payment is null)
        {
            return new PaymentContractResult(
                false, transactionId, PaymentStatus.Failed,
                $"Payment {transactionId} not found.", null, DateTime.UtcNow);
        }

        payment.Complete();
        var confirmationCode = $"CONF-{Guid.NewGuid():N}"[..16];

        _logger.LogInformation(
            "Payment {TransactionId} confirmed by {ConfirmedBy}, code {Confirmation}",
            transactionId, confirmedBy, confirmationCode);

        return new PaymentContractResult(
            true, transactionId, PaymentStatus.Completed,
            "Payment confirmed successfully.", confirmationCode, DateTime.UtcNow);
    }

    public async Task<PaymentContractResult> DeclinePaymentAsync(
        string transactionId,
        string reason,
        string declinedBy,
        CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(Guid.Parse(transactionId), cancellationToken);
        if (payment is null)
        {
            return new PaymentContractResult(
                false, transactionId, PaymentStatus.Failed,
                $"Payment {transactionId} not found.", null, DateTime.UtcNow);
        }

        payment.Fail(reason);

        _logger.LogWarning(
            "Payment {TransactionId} declined by {DeclinedBy}: {Reason}",
            transactionId, declinedBy, reason);

        return new PaymentContractResult(
            false, transactionId, PaymentStatus.Failed,
            $"Payment declined: {reason}", null, DateTime.UtcNow);
    }

    public async Task<IReadOnlyCollection<PendingReconciliationItem>> GetPendingReconciliationAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        var payments = await _paymentRepository.GetFilteredAsync(null, null, from, to, 0, int.MaxValue, cancellationToken);

        var pending = payments
            .Where(p => p.Status != PaymentStatus.Failed)
            .Select(p => new PendingReconciliationItem(
                p.Id.ToString(),
                MapPaymentMethodToProvider(p.PaymentMethod),
                p.Amount,
                p.Currency,
                p.CustomerId.ToString(),
                p.PaidAt,
                p.Status))
            .ToList();

        return pending.AsReadOnly();
    }

    private static PaymentProvider MapPaymentMethodToProvider(PaymentMethodType method)
    {
        return method switch
        {
            PaymentMethodType.BankTransfer => PaymentProvider.LocalBank,
            PaymentMethodType.MobileMoney => PaymentProvider.MobileMoney,
            PaymentMethodType.Cash => PaymentProvider.Cash,
            _ => PaymentProvider.LocalBank
        };
    }
}
