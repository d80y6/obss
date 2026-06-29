using Obss.SharedKernel.Domain.Common;

namespace Obss.SharedKernel.Domain.ValueObjects;

public sealed class TenantId : ValueObject
{
    private TenantId(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static TenantId Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Tenant ID cannot be empty", nameof(value));

        return new TenantId(value);
    }

    public static TenantId New() => new(Guid.NewGuid().ToString("N"));

    public override string ToString() => Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}