using Microsoft.EntityFrameworkCore;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Domain.Entities;
using Obss.AAA.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.AAA.Infrastructure.Persistence.Repositories;

public sealed class NasRepository : EfRepository<NetworkAccessServer>, INasRepository
{
    public NasRepository(AaaDbContext context) : base(context)
    {
    }

    public async Task<NetworkAccessServer?> GetByIpAddressAsync(string ipAddress, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(n => n.NasIpAddress == ipAddress, cancellationToken);
    }

    public async Task<IReadOnlyList<NetworkAccessServer>> GetByNasTypeAsync(NasType nasType, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(n => n.NasType == nasType).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<NetworkAccessServer>> GetActiveNasDevicesAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(n => n.Status == "Active").ToListAsync(cancellationToken);
    }
}
