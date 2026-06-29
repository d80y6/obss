using System.Text.RegularExpressions;
using Obss.SharedKernel.Domain.Common;

namespace Obss.ModuleTemplate.Domain.ValueObjects;

public sealed partial class SampleValueObject : ValueObject
{
    private static readonly Regex FormatPattern = FormatRegex();

    private SampleValueObject(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static SampleValueObject Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be empty", nameof(value));

        if (!FormatPattern.IsMatch(value))
            throw new ArgumentException($"Invalid format: '{value}'", nameof(value));

        return new SampleValueObject(value.ToLowerInvariant());
    }

    public override string ToString() => Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex(@"^[a-z][a-z0-9]*\.[a-z][a-z0-9]*$", RegexOptions.Compiled)]
    private static partial Regex FormatRegex();
}
