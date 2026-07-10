using Microsoft.EntityFrameworkCore;
using Obss.ServiceQualification.Application.Abstractions;
using Obss.ServiceQualification.Infrastructure.Persistence;

namespace Obss.ServiceQualification.Infrastructure.Persistence.Repositories;

public class ServiceQualificationRepository : IServiceQualificationRepository
{
    private readonly ServiceQualificationDbContext _context;

    public ServiceQualificationRepository(ServiceQualificationDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Entities.ServiceQualification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Domain.Entities.ServiceQualification>()
            .Include(x => x.Items)
            .ThenInclude(x => x.AlternateProposals)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<Domain.Entities.ServiceQualification>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Domain.Entities.ServiceQualification>()
            .Include(x => x.Items)
            .ThenInclude(x => x.AlternateProposals)
            .Where(x => x.CustomerId == customerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Domain.Entities.ServiceQualification>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<Domain.Entities.ServiceQualification>()
            .Include(x => x.Items)
            .ThenInclude(x => x.AlternateProposals)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Domain.Entities.ServiceQualification qualification, CancellationToken cancellationToken = default)
    {
        await _context.Set<Domain.Entities.ServiceQualification>().AddAsync(qualification, cancellationToken);
    }
}
