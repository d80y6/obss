using Microsoft.Extensions.Logging;
using Obss.Payments.Application.Abstractions;
using Obss.Payments.Domain.Services;
using Obss.Payments.Domain.ValueObjects;

namespace Obss.Payments.Infrastructure.Services.Gateways;

public sealed class StripeGateway : IGatewayClient
{
    private readonly ILogger<StripeGateway> _logger;

    public StripeGateway(ILogger<StripeGateway> logger)
    {
        _logger = logger;
    }

    public PaymentProvider Provider => PaymentProvider.Stripe;

    public Task<PaymentResult> ProcessAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stripe: Processing payment of {Amount} {Currency}", request.Amount, request.Currency);

        var result = new PaymentResult(
            true,
            $"stripe_{Guid.NewGuid():N}",
            PaymentStatus.Completed,
            "Payment processed via Stripe.",
            null);

        return Task.FromResult(result);
    }

    public Task<RefundResult> RefundAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stripe: Refunding {Amount} from transaction {TransactionId}", amount, transactionId);

        var result = new RefundResult(
            true,
            $"refund_{Guid.NewGuid():N}",
            PaymentStatus.Refunded,
            "Refund processed via Stripe.");

        return Task.FromResult(result);
    }

    public Task<PaymentStatus> VerifyAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stripe: Verifying transaction {TransactionId}", transactionId);
        return Task.FromResult(PaymentStatus.Completed);
    }
}
