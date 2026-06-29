using Obss.Payments.Domain.ValueObjects;

namespace Obss.Payments.Domain.Services;

public interface IPaymentGatewayService
{
    Task<PaymentResult> ProcessPayment(PaymentRequest request, CancellationToken cancellationToken = default);
    Task<RefundResult> RefundPayment(string transactionId, decimal amount, CancellationToken cancellationToken = default);
    Task<PaymentStatus> VerifyPayment(string transactionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PaymentGatewayInfo>> GetSupportedGateways(CancellationToken cancellationToken = default);
}
