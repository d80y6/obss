using Obss.Payments.Domain.Services;
using Obss.Payments.Domain.ValueObjects;

namespace Obss.Payments.Application.Abstractions;

public interface IGatewayClient
{
    PaymentProvider Provider { get; }
    Task<PaymentResult> ProcessAsync(PaymentRequest request, CancellationToken cancellationToken = default);
    Task<RefundResult> RefundAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default);
    Task<PaymentStatus> VerifyAsync(string transactionId, CancellationToken cancellationToken = default);
}
