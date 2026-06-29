using Microsoft.EntityFrameworkCore;
using Obss.IAM.Application.Abstractions;
using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.IAM.Infrastructure.Persistence.Repositories;

public sealed class TenantRepository : EfRepository<Tenant>, ITenantRepository
{
    public TenantRepository(IamDbContext context)
        : base(context)
    {
    }

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);
    }
}
