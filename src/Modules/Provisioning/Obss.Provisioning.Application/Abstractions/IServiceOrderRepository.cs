using Obss.Provisioning.Domain.Entities;
using Obss.SharedKernel.Domain.Specifications;

namespace Obss.Provisioning.Application.Abstractions;

public interface IServiceOrderRepository
{
    Task<ServiceOrder?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ServiceOrder>> GetListAsync(Specification<ServiceOrder>? spec = null, CancellationToken ct = default);
    Task AddAsync(ServiceOrder order, CancellationToken ct = default);
    Task UpdateAsync(ServiceOrder order, CancellationToken ct = default);
    Task DeleteAsync(ServiceOrder order, CancellationToken ct = default);
    Task<int> CountAsync(Specification<ServiceOrder>? spec = null, CancellationToken ct = default);
}
