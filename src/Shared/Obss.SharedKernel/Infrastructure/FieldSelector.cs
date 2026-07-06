using System.Text.Json;
using System.Text.Json.Serialization;

namespace Obss.SharedKernel.Infrastructure;

public static class FieldSelector
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string SelectFields(object source, string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
            return JsonSerializer.Serialize(source, Options);

        var fieldNames = fields.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var sourceDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            JsonSerializer.Serialize(source, Options), Options);

        var filtered = sourceDict!
            .Where(kvp => fieldNames.Contains(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return JsonSerializer.Serialize(filtered, Options);
    }
}
