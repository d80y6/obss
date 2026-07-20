using Microsoft.EntityFrameworkCore;
using Obss.ServiceQualification.Domain.Abstractions;
using Obss.ServiceQualification.Domain.Entities;
using Obss.ServiceQualification.Domain.ValueObjects;
using Obss.ServiceQualification.Infrastructure.Persistence;

namespace Obss.ServiceQualification.Infrastructure.Persistence.Repositories;

public class CoverageAreaRepository : ICoverageAreaRepository
{
    private readonly ServiceQualificationDbContext _context;

    public CoverageAreaRepository(ServiceQualificationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CoverageArea>> GetByAddressAsync(GeographicAddress address, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<CoverageArea>()
            .Include(ca => ca.AvailableServices)
            .Where(ca => ca.City == address.City);

        if (!string.IsNullOrWhiteSpace(address.State))
            query = query.Where(ca => ca.State == address.State || ca.State == null);

        if (!string.IsNullOrWhiteSpace(address.PostalCode))
            query = query.Where(ca => ca.PostalCode == address.PostalCode || ca.PostalCode == null);

        // Street range check: match if either street range is null (covers whole city) or address is within range
        if (!string.IsNullOrWhiteSpace(address.Street))
        {
            query = query.Where(ca =>
                ca.StreetFrom == null ||
                string.Compare(address.Street, ca.StreetFrom, StringComparison.OrdinalIgnoreCase) >= 0)
                .Where(ca =>
                    ca.StreetTo == null ||
                    string.Compare(address.Street, ca.StreetTo, StringComparison.OrdinalIgnoreCase) <= 0);
        }

        return await query.ToListAsync(cancellationToken);
    }
}
