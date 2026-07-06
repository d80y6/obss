using Obss.Provisioning.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Provisioning.Application.Abstractions;

public interface IProvisioningJobRepository : IRepository<ProvisioningJob>
{
    Task<ProvisioningJob?> GetByIdWithTasksAsync(Guid jobId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProvisioningJob>> GetFilteredAsync(
        Guid? orderId,
        string? status,
        Guid? serviceId,
        int offset,
        int limit,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProvisioningJob>> GetQueuedJobsAsync(CancellationToken cancellationToken = default);
}
