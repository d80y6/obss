using Microsoft.Extensions.Logging;
using Obss.Payments.Application.Abstractions;
using Obss.Payments.Domain.Services;
using Obss.Payments.Domain.ValueObjects;

namespace Obss.Payments.Infrastructure.Services.Gateways;

public sealed class CashGateway : IGatewayClient
{
    private readonly ILogger<CashGateway> _logger;

    public CashGateway(ILogger<CashGateway> logger)
    {
        _logger = logger;
    }

    public PaymentProvider Provider => PaymentProvider.Cash;

    public Task<PaymentResult> ProcessAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cash: Recording cash payment of {Amount} {Currency}", request.Amount, request.Currency);

        var result = new PaymentResult(
            true,
            $"cash_{Guid.NewGuid():N}",
            PaymentStatus.Completed,
            "Cash payment recorded.",
            null);

        return Task.FromResult(result);
    }

    public Task<RefundResult> RefundAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cash: Refunding {Amount} from transaction {TransactionId}", amount, transactionId);

        var result = new RefundResult(
            true,
            $"refund_{Guid.NewGuid():N}",
            PaymentStatus.Refunded,
            "Cash refund processed.");

        return Task.FromResult(result);
    }

    public Task<PaymentStatus> VerifyAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cash: Verifying transaction {TransactionId}", transactionId);
        return Task.FromResult(PaymentStatus.Completed);
    }
}
