using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Abstractions;

public interface IBillQuery
{
    Task<Result<BillDto>> GetBillByIdAsync(Guid billId, CancellationToken cancellationToken = default);
}
