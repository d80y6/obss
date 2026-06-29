using Microsoft.EntityFrameworkCore;
using Obss.ApiGateway.Application.Abstractions;
using Obss.ApiGateway.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ApiGateway.Infrastructure.Persistence.Repositories;

public sealed class PartnerRepository : EfRepository<Partner>, IPartnerRepository
{
    public PartnerRepository(GatewayDbContext context)
        : base(context)
    {
    }

    public async Task<Partner?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(p => p.Name == name, cancellationToken);
    }
}
