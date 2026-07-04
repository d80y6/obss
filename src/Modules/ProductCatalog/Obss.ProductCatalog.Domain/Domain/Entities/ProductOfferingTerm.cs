using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.ProductCatalog.Domain.Domain.Entities;

public class ProductOfferingTerm : Entity<Guid>
{
    private ProductOfferingTerm() { }

    public ProductOfferingTerm(
        Guid id,
        Guid offerId,
        string name,
        string? description,
        int duration,
        DurationUnit durationUnit,
        TermType termType,
        DateTime? validFrom,
        DateTime? validTo)
        : base(id)
    {
        OfferId = offerId;
        Name = name;
        Description = description;
        Duration = duration;
        DurationUnit = durationUnit;
        TermType = termType;
        ValidFrom = validFrom;
        ValidTo = validTo;
    }

    public Guid OfferId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int Duration { get; private set; }
    public DurationUnit DurationUnit { get; private set; }
    public TermType TermType { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }

    public void Update(string name, string? description, int duration, DurationUnit durationUnit, TermType termType, DateTime? validFrom, DateTime? validTo)
    {
        Name = name;
        Description = description;
        Duration = duration;
        DurationUnit = durationUnit;
        TermType = termType;
        ValidFrom = validFrom;
        ValidTo = validTo;
    }
}
