using Obss.SharedKernel.Domain.Common;

namespace Obss.Subscriptions.Domain.Entities;

public class ProductCharacteristic : Entity<Guid>
{
    private ProductCharacteristic() { }

    public ProductCharacteristic(Guid id, string name, string value, string? valueType = null)
        : base(id)
    {
        Name = name;
        Value = value;
        ValueType = valueType;
    }

    public string Name { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public string? ValueType { get; private set; }
}
