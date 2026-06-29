using Obss.SharedKernel.Domain.Common;

namespace Obss.ProductCatalog.Domain.Domain.ValueObjects;

public sealed class ProductSpecification : ValueObject
{
    private ProductSpecification() { }

    public ProductSpecification(string name, string value, bool isRequired)
    {
        Name = name;
        Value = value;
        IsRequired = isRequired;
    }

    public string Name { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public bool IsRequired { get; private set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return Value;
        yield return IsRequired;
    }
}
