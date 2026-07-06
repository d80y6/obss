using Obss.SharedKernel.Domain.Common;

namespace Obss.ServiceCatalog.Domain.Entities;

public class ServiceSpecCharValue : Entity<Guid>
{
    public Guid CharacteristicId { get; private set; }
    public string Value { get; private set; } = string.Empty;
    public string? UnitOfMeasure { get; private set; }
    public bool IsDefault { get; private set; }
    public string? ValueFrom { get; private set; }
    public string? ValueTo { get; private set; }
    public string? RangeInterval { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }

    private ServiceSpecCharValue() { }

    public ServiceSpecCharValue(Guid id, Guid characteristicId, string value, string? unitOfMeasure, bool isDefault, string? valueFrom, string? valueTo, string? rangeInterval, DateTime? validFrom, DateTime? validTo) : base(id)
    {
        CharacteristicId = characteristicId;
        Value = value;
        UnitOfMeasure = unitOfMeasure;
        IsDefault = isDefault;
        ValueFrom = valueFrom;
        ValueTo = valueTo;
        RangeInterval = rangeInterval;
        ValidFrom = validFrom;
        ValidTo = validTo;
    }

    public void Update(string value, string? unitOfMeasure, bool isDefault, string? valueFrom, string? valueTo, string? rangeInterval, DateTime? validFrom, DateTime? validTo)
    {
        Value = value;
        UnitOfMeasure = unitOfMeasure;
        IsDefault = isDefault;
        ValueFrom = valueFrom;
        ValueTo = valueTo;
        RangeInterval = rangeInterval;
        ValidFrom = validFrom;
        ValidTo = validTo;
    }
}
