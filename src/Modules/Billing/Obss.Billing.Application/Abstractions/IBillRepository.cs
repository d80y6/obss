using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Billing.Application.Abstractions;

public interface IBillRepository : IRepository<Bill>
{
    Task<Bill?> GetByIdWithLinesAsync(Guid billId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Bill>> GetByCustomerAsync(
        Guid? customerId,
        BillStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        int offset,
        int limit,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Bill>> GetOpenBillsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Bill>> GetBillsDueForGenerationAsync(DateTime upToDate, CancellationToken cancellationToken = default);
}
