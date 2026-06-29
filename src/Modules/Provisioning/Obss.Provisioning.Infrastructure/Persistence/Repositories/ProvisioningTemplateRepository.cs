using Microsoft.EntityFrameworkCore;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Provisioning.Infrastructure.Persistence.Repositories;

public sealed class ProvisioningTemplateRepository : EfRepository<ProvisioningTemplate>, IProvisioningTemplateRepository
{
    public ProvisioningTemplateRepository(ProvisioningDbContext context)
        : base(context)
    {
    }

    public async Task<ProvisioningTemplate?> GetByServiceTypeAndActionAsync(
        string serviceType, string action, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(t =>
                t.ServiceType == serviceType &&
                t.Action == action &&
                t.IsActive,
                cancellationToken);
    }
}
