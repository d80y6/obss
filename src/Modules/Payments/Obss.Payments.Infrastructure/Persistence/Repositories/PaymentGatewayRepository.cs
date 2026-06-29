using Microsoft.EntityFrameworkCore;
using Obss.Payments.Application.Abstractions;
using Obss.Payments.Domain.Entities;
using Obss.Payments.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Payments.Infrastructure.Persistence.Repositories;

public sealed class PaymentGatewayRepository : EfRepository<PaymentGateway>, IPaymentGatewayRepository
{
    public PaymentGatewayRepository(PaymentDbContext context)
        : base(context)
    {
    }

    public async Task<PaymentGateway?> GetByProviderAsync(PaymentProvider provider, string tenantId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(g => g.Provider == provider && g.TenantId == tenantId, cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentGateway>> GetActiveGatewaysAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(g => g.TenantId == tenantId && g.IsActive)
            .ToListAsync(cancellationToken);
    }
}
