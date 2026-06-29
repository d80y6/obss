using Microsoft.Extensions.Logging;
using Obss.Payments.Application.Abstractions;
using Obss.Payments.Domain.Services;
using Obss.Payments.Domain.ValueObjects;

namespace Obss.Payments.Application.Services;

public sealed class PaymentGatewayManager : IPaymentGatewayService
{
    private readonly IEnumerable<IGatewayClient> _gatewayClients;
    private readonly ILogger<PaymentGatewayManager> _logger;

    public PaymentGatewayManager(
        IEnumerable<IGatewayClient> gatewayClients,
        ILogger<PaymentGatewayManager> logger)
    {
        _gatewayClients = gatewayClients;
        _logger = logger;
    }

    public async Task<PaymentResult> ProcessPayment(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        var client = GetClient(request.PaymentMethod);

        if (client is null)
        {
            _logger.LogWarning("No gateway client found for payment method {PaymentMethod}", request.PaymentMethod);
            return new PaymentResult(false, string.Empty, PaymentStatus.Failed, $"No gateway configured for method '{request.PaymentMethod}'", null);
        }

        _logger.LogInformation("Processing payment of {Amount} {Currency} via {Provider}", request.Amount, request.Currency, client.Provider);

        return await client.ProcessAsync(request, cancellationToken);
    }

    public async Task<RefundResult> RefundPayment(string transactionId, decimal amount, CancellationToken cancellationToken = default)
    {
        foreach (var client in _gatewayClients)
        {
            try
            {
                return await client.RefundAsync(transactionId, amount, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Refund via {Provider} failed, trying next gateway.", client.Provider);
            }
        }

        return new RefundResult(false, string.Empty, PaymentStatus.Failed, "All gateways failed to process refund.");
    }

    public async Task<PaymentStatus> VerifyPayment(string transactionId, CancellationToken cancellationToken = default)
    {
        foreach (var client in _gatewayClients)
        {
            try
            {
                return await client.VerifyAsync(transactionId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Verification via {Provider} failed, trying next gateway.", client.Provider);
            }
        }

        return PaymentStatus.Failed;
    }

    public Task<IEnumerable<PaymentGatewayInfo>> GetSupportedGateways(CancellationToken cancellationToken = default)
    {
        var gateways = _gatewayClients.Select(c => new PaymentGatewayInfo(
            c.Provider,
            c.Provider.ToString(),
            []));

        return Task.FromResult(gateways);
    }

    private IGatewayClient? GetClient(string paymentMethod)
    {
        if (Enum.TryParse<PaymentProvider>(paymentMethod, true, out var provider))
        {
            return _gatewayClients.FirstOrDefault(c => c.Provider == provider);
        }

        return _gatewayClients.FirstOrDefault(c =>
            c.Provider.ToString().Equals(paymentMethod, StringComparison.OrdinalIgnoreCase));
    }
}
