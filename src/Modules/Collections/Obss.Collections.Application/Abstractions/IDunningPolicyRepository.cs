using Obss.Collections.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Collections.Application.Abstractions;

public interface IDunningPolicyRepository : IRepository<DunningPolicy>
{
    Task<DunningPolicy?> GetActivePolicyAsync(CancellationToken cancellationToken = default);
}
