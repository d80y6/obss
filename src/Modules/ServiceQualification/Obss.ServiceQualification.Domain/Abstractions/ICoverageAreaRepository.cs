using Obss.ServiceQualification.Domain.Entities;
using Obss.ServiceQualification.Domain.ValueObjects;

namespace Obss.ServiceQualification.Domain.Abstractions;

public interface ICoverageAreaRepository
{
    Task<List<CoverageArea>> GetByAddressAsync(GeographicAddress address, CancellationToken cancellationToken = default);
}
