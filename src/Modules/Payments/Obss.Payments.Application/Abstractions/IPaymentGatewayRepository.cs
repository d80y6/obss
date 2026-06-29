using Obss.Payments.Domain.Entities;
using Obss.Payments.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Payments.Application.Abstractions;

public interface IPaymentGatewayRepository : IRepository<PaymentGateway>
{
    Task<PaymentGateway?> GetByProviderAsync(PaymentProvider provider, string tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentGateway>> GetActiveGatewaysAsync(string tenantId, CancellationToken cancellationToken = default);
}
