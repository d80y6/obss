using Microsoft.Extensions.Logging;
using Obss.Payments.Application.Abstractions;
using Obss.Payments.Domain.Services;
using Obss.Payments.Domain.ValueObjects;

namespace Obss.Payments.Infrastructure.Services.Gateways;

public sealed class LocalBankGateway : IGatewayClient
{
    private readonly ILogger<LocalBankGateway> _logger;

    public LocalBankGateway(ILogger<LocalBankGateway> logger)
    {
        _logger = logger;
    }

    public PaymentProvider Provider => PaymentProvider.LocalBank;

    public Task<PaymentResult> ProcessAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("LocalBank: Processing payment of {Amount} {Currency}", request.Amount, request.Currency);

        var result = new PaymentResult(
            true,
            $"bank_{Guid.NewGuid():N}",
            PaymentStatus.Completed,
            "Payment processed via Local Bank Transfer.",
            null);

        return Task.FromResult(result);
    }

    public Task<RefundResult> RefundAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("LocalBank: Refunding {Amount} from transaction {TransactionId}", amount, transactionId);

        var result = new RefundResult(
            true,
            $"refund_{Guid.NewGuid():N}",
            PaymentStatus.Refunded,
            "Refund processed via Local Bank.");

        return Task.FromResult(result);
    }

    public Task<PaymentStatus> VerifyAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("LocalBank: Verifying transaction {TransactionId}", transactionId);
        return Task.FromResult(PaymentStatus.Completed);
    }
}
