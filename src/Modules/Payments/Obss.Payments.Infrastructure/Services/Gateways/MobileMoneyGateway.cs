using Microsoft.Extensions.Logging;
using Obss.Payments.Application.Abstractions;
using Obss.Payments.Domain.Services;
using Obss.Payments.Domain.ValueObjects;

namespace Obss.Payments.Infrastructure.Services.Gateways;

public sealed class MobileMoneyGateway : IGatewayClient
{
    private readonly ILogger<MobileMoneyGateway> _logger;

    public MobileMoneyGateway(ILogger<MobileMoneyGateway> logger)
    {
        _logger = logger;
    }

    public PaymentProvider Provider => PaymentProvider.MobileMoney;

    public Task<PaymentResult> ProcessAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MobileMoney: Processing payment of {Amount} {Currency}", request.Amount, request.Currency);

        var result = new PaymentResult(
            true,
            $"mobile_{Guid.NewGuid():N}",
            PaymentStatus.Completed,
            "Payment processed via Mobile Money.",
            null);

        return Task.FromResult(result);
    }

    public Task<RefundResult> RefundAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MobileMoney: Refunding {Amount} from transaction {TransactionId}", amount, transactionId);

        var result = new RefundResult(
            true,
            $"refund_{Guid.NewGuid():N}",
            PaymentStatus.Refunded,
            "Refund processed via Mobile Money.");

        return Task.FromResult(result);
    }

    public Task<PaymentStatus> VerifyAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MobileMoney: Verifying transaction {TransactionId}", transactionId);
        return Task.FromResult(PaymentStatus.Completed);
    }
}
