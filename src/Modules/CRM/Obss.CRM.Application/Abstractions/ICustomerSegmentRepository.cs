using Obss.CRM.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.CRM.Application.Abstractions;

public interface ICustomerSegmentRepository : IRepository<CustomerSegment>
{
    Task<IReadOnlyList<CustomerSegment>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<int> GetCustomerCountAsync(Guid segmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerSegmentAssignment>> GetAssignmentsAsync(Guid segmentId, CancellationToken cancellationToken = default);
    Task<bool> IsCustomerInSegmentAsync(Guid segmentId, Guid customerId, CancellationToken cancellationToken = default);
}
