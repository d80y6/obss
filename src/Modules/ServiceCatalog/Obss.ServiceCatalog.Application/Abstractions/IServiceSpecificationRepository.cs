using Obss.ServiceCatalog.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.ServiceCatalog.Application.Abstractions;

public interface IServiceSpecificationRepository : IRepository<ServiceSpecification>
{
    Task<ServiceSpecification?> GetByIdWithCharacteristicsAsync(Guid id, CancellationToken ct = default);
    Task<List<ServiceSpecification>> GetByBrandAsync(string brand, CancellationToken ct = default);
}
