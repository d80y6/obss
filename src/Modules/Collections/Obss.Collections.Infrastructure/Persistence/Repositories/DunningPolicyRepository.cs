using Microsoft.EntityFrameworkCore;
using Obss.Collections.Application.Abstractions;
using Obss.Collections.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Collections.Infrastructure.Persistence.Repositories;

public sealed class DunningPolicyRepository : EfRepository<DunningPolicy>, IDunningPolicyRepository
{
    public DunningPolicyRepository(CollectionDbContext context)
        : base(context)
    {
    }

    public async Task<DunningPolicy?> GetActivePolicyAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.IsActive)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
