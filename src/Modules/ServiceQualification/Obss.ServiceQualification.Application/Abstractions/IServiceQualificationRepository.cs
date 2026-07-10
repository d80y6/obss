using Obss.ServiceQualification.Domain.Entities;

namespace Obss.ServiceQualification.Application.Abstractions;

public interface IServiceQualificationRepository
{
    Task<Domain.Entities.ServiceQualification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Domain.Entities.ServiceQualification>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<List<Domain.Entities.ServiceQualification>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Entities.ServiceQualification qualification, CancellationToken cancellationToken = default);
}
