using Obss.SharedKernel.Domain.Common;

namespace Obss.ServiceCatalog.Domain.Entities;

public class ServiceSpecCharacteristic : Entity<Guid>
{
    private readonly List<ServiceSpecCharValue> _values = [];

    public Guid ServiceSpecificationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string ValueType { get; private set; } = "string";
    public bool Configurable { get; private set; } = true;
    public decimal? MinValue { get; private set; }
    public decimal? MaxValue { get; private set; }
    public string? Regex { get; private set; }
    public int SortOrder { get; private set; }
    public int? MaxCardinality { get; private set; }
    public bool IsRequired { get; private set; }
    public IReadOnlyCollection<ServiceSpecCharValue> Values => _values.AsReadOnly();

    private ServiceSpecCharacteristic() { }

    public ServiceSpecCharacteristic(Guid id, Guid serviceSpecificationId, string name, string? description, string valueType, bool configurable, decimal? minValue, decimal? maxValue, string? regex, int sortOrder, int? maxCardinality, bool isRequired) : base(id)
    {
        ServiceSpecificationId = serviceSpecificationId;
        Name = name;
        Description = description;
        ValueType = valueType;
        Configurable = configurable;
        MinValue = minValue;
        MaxValue = maxValue;
        Regex = regex;
        SortOrder = sortOrder;
        MaxCardinality = maxCardinality;
        IsRequired = isRequired;
    }

    public void UpdateDetails(string name, string? description, string valueType, bool configurable, decimal? minValue, decimal? maxValue, string? regex, int sortOrder, int? maxCardinality, bool isRequired)
    {
        Name = name;
        Description = description;
        ValueType = valueType;
        Configurable = configurable;
        MinValue = minValue;
        MaxValue = maxValue;
        Regex = regex;
        SortOrder = sortOrder;
        MaxCardinality = maxCardinality;
        IsRequired = isRequired;
    }

    public void AddValue(ServiceSpecCharValue value)
    {
        _values.Add(value);
    }

    public void RemoveValue(Guid valueId)
    {
        _values.RemoveAll(v => v.Id == valueId);
    }
}
