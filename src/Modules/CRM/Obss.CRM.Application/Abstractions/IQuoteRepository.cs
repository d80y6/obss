using Obss.CRM.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.CRM.Application.Abstractions;

public interface IQuoteRepository : IRepository<Quote>
{
    Task<Quote?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Quote>> GetListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Quote>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
}
