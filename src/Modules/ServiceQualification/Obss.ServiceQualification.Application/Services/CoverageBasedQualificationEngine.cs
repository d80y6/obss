using Obss.ServiceQualification.Application.Abstractions;
using Obss.ServiceQualification.Domain.ValueObjects;

namespace Obss.ServiceQualification.Application.Services;

public class CoverageBasedQualificationEngine : IServiceQualificationEngine
{
    private readonly ICoverageAreaRepository _coverageAreaRepository;

    public CoverageBasedQualificationEngine(ICoverageAreaRepository coverageAreaRepository)
    {
        _coverageAreaRepository = coverageAreaRepository;
    }

    public async Task<QualificationEngineResult> QualifyAsync(
        GeographicAddress address,
        IReadOnlyList<QualificationRequestItem> requestedServices,
        CancellationToken cancellationToken)
    {
        var coverageAreas = await _coverageAreaRepository.GetByAddressAsync(address, cancellationToken);

        var itemResults = new List<QualificationEngineItemResult>();

        foreach (var requested in requestedServices)
        {
            var matchingServices = coverageAreas
                .SelectMany(ca => ca.AvailableServices)
                .Where(cs => cs.IsActive)
                .ToList();

            var exactMatch = matchingServices.FirstOrDefault(cs =>
                cs.ServiceName.Equals(requested.ServiceName, StringComparison.OrdinalIgnoreCase));

            QualificationEngineItemResult result;

            if (exactMatch is not null)
            {
                result = new QualificationEngineItemResult(
                    requested.ServiceId,
                    requested.ServiceName,
                    QualificationResultType.Qualified,
                    DateTime.UtcNow.AddDays(7),
                    DateTime.UtcNow.AddDays(14),
                    null,
                    []);
            }
            else if (matchingServices.Count > 0)
            {
                var alternatives = matchingServices
                    .Select(cs => new AlternateProposalResult(
                        Guid.NewGuid(),
                        cs.ServiceName,
                        QualificationResultType.Qualified,
                        DateTime.UtcNow.AddDays(7),
                        DateTime.UtcNow.AddDays(30)))
                    .ToList();

                result = new QualificationEngineItemResult(
                    requested.ServiceId,
                    requested.ServiceName,
                    QualificationResultType.Alternate,
                    null,
                    null,
                    "Service not available at requested speed/technology.",
                    alternatives);
            }
            else
            {
                result = new QualificationEngineItemResult(
                    requested.ServiceId,
                    requested.ServiceName,
                    QualificationResultType.Unqualified,
                    null,
                    null,
                    "No coverage available at this location.",
                    []);
            }

            itemResults.Add(result);
        }

        return new QualificationEngineResult(itemResults);
    }
}
