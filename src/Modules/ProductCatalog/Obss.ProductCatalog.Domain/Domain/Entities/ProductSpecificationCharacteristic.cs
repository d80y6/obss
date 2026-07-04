using Obss.SharedKernel.Domain.Common;

namespace Obss.ProductCatalog.Domain.Domain.Entities;

public class ProductSpecificationCharacteristic : Entity<Guid>
{
    private readonly List<ProductSpecificationCharacteristicValue> _values = [];

    private ProductSpecificationCharacteristic() { }

    public ProductSpecificationCharacteristic(
        Guid id,
        Guid productSpecificationId,
        string name,
        string? description,
        string valueType,
        bool configurable,
        int? minValue,
        int? maxValue,
        string? regex,
        int sortOrder,
        int? maxCardinality,
        bool isRequired)
        : base(id)
    {
        ProductSpecificationId = productSpecificationId;
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

    public Guid ProductSpecificationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string ValueType { get; private set; } = "string";
    public bool Configurable { get; private set; } = true;
    public int? MinValue { get; private set; }
    public int? MaxValue { get; private set; }
    public string? Regex { get; private set; }
    public int SortOrder { get; private set; }
    public int? MaxCardinality { get; private set; }
    public bool IsRequired { get; private set; }

    public IReadOnlyCollection<ProductSpecificationCharacteristicValue> Values => _values.AsReadOnly();

    public void UpdateDetails(
        string name,
        string? description,
        string valueType,
        bool configurable,
        int? minValue,
        int? maxValue,
        string? regex,
        int sortOrder,
        int? maxCardinality,
        bool isRequired)
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

    public void AddValue(ProductSpecificationCharacteristicValue value)
    {
        _values.Add(value);
    }

    public void RemoveValue(ProductSpecificationCharacteristicValue value)
    {
        _values.Remove(value);
    }
}
