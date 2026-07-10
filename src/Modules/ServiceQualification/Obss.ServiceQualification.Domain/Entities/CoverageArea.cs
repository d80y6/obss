using Obss.SharedKernel.Domain.Common;
using Obss.ServiceQualification.Domain.ValueObjects;

namespace Obss.ServiceQualification.Domain.Entities;

public class CoverageArea : Entity<Guid>
{
    private readonly List<CoverageService> _availableServices = [];

    public string City { get; private set; } = null!;
    public string? State { get; private set; }
    public string? StreetFrom { get; private set; }
    public string? StreetTo { get; private set; }
    public string? PostalCode { get; private set; }
    public IReadOnlyCollection<CoverageService> AvailableServices => _availableServices.AsReadOnly();

    private CoverageArea() { }

    public CoverageArea(
        Guid id,
        string city,
        string? state,
        string? streetFrom,
        string? streetTo,
        string? postalCode) : base(id)
    {
        City = city;
        State = state;
        StreetFrom = streetFrom;
        StreetTo = streetTo;
        PostalCode = postalCode;
    }

    public void AddService(CoverageService service)
    {
        _availableServices.Add(service);
    }
}
