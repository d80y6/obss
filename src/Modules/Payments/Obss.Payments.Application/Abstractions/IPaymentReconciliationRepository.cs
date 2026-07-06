using Obss.Payments.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Payments.Application.Abstractions;

public interface IPaymentReconciliationRepository : IRepository<PaymentReconciliation>
{
    Task<PaymentReconciliation?> GetWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentReconciliation>> GetByStatusAsync(string status, string tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentReconciliation>> GetFilteredAsync(string? status, DateTime? fromDate, DateTime? toDate, int offset = 0, int limit = 20, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReconciliationItem>> GetUnmatchedItemsAsync(string tenantId, CancellationToken cancellationToken = default);
}
