using Obss.SharedKernel.Domain.Common;

namespace Obss.SharedKernel.Domain.ValueObjects;

public sealed class Email : ValueObject
{
    private Email(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty", nameof(value));

        if (!value.Contains('@') || !value.Contains('.'))
            throw new ArgumentException($"Invalid email format: {value}", nameof(value));

        return new Email(value.ToLowerInvariant().Trim());
    }

    public override string ToString() => Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}