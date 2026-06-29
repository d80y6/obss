using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.IAM.Application.Abstractions;

public interface ITenantRepository : IRepository<Tenant>
{
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
