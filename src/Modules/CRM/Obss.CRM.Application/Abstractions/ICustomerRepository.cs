using Obss.CRM.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.CRM.Application.Abstractions;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<IReadOnlyList<Customer>> GetFilteredAsync(
        string? tenantId,
        string? status,
        string? customerType,
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Contact>> GetContactsByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustomerNote>> GetNotesByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<int> GetFilteredCountAsync(
        string? tenantId,
        string? status,
        string? customerType,
        string? searchTerm,
        CancellationToken cancellationToken = default);
}
