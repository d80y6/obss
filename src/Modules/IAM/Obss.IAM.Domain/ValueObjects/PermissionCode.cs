using System.Text.RegularExpressions;
using Obss.SharedKernel.Domain.Common;

namespace Obss.IAM.Domain.ValueObjects;

public sealed partial class PermissionCode : ValueObject
{
    private static readonly Regex CodePattern = CodeRegex();

    private PermissionCode(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PermissionCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Permission code cannot be empty", nameof(value));

        var normalized = value.ToLowerInvariant();

        if (!CodePattern.IsMatch(normalized))
            throw new ArgumentException($"Invalid permission code format: '{value}'. Expected format: '{{module}}.{{resource}}.{{action}}'", nameof(value));

        return new PermissionCode(normalized);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex(@"^[a-z][a-z0-9]*\.[a-z][a-z0-9]*\.[a-z][a-z0-9]*$", RegexOptions.Compiled)]
    private static partial Regex CodeRegex();
}
