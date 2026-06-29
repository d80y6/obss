using Microsoft.Extensions.Logging;
using Obss.Payments.Application.Abstractions;
using Obss.Payments.Domain.Services;
using Obss.Payments.Domain.ValueObjects;

namespace Obss.Payments.Infrastructure.Services.Gateways;

public sealed class PayPalGateway : IGatewayClient
{
    private readonly ILogger<PayPalGateway> _logger;

    public PayPalGateway(ILogger<PayPalGateway> logger)
    {
        _logger = logger;
    }

    public PaymentProvider Provider => PaymentProvider.PayPal;

    public Task<PaymentResult> ProcessAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("PayPal: Processing payment of {Amount} {Currency}", request.Amount, request.Currency);

        var result = new PaymentResult(
            true,
            $"paypal_{Guid.NewGuid():N}",
            PaymentStatus.Completed,
            "Payment processed via PayPal.",
            null);

        return Task.FromResult(result);
    }

    public Task<RefundResult> RefundAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("PayPal: Refunding {Amount} from transaction {TransactionId}", amount, transactionId);

        var result = new RefundResult(
            true,
            $"refund_{Guid.NewGuid():N}",
            PaymentStatus.Refunded,
            "Refund processed via PayPal.");

        return Task.FromResult(result);
    }

    public Task<PaymentStatus> VerifyAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("PayPal: Verifying transaction {TransactionId}", transactionId);
        return Task.FromResult(PaymentStatus.Completed);
    }
}
