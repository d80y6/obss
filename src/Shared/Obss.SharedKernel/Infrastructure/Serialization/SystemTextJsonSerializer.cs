using System.Text.Json;
using System.Text.Json.Serialization;

namespace Obss.SharedKernel.Infrastructure.Serialization;

public static class SystemTextJsonSerializer
{
    public static JsonSerializerOptions DefaultOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    public static string Serialize<T>(T value)
        where T : notnull
        => JsonSerializer.Serialize(value, DefaultOptions);

    public static object? Deserialize(string json, Type type)
        => JsonSerializer.Deserialize(json, type, DefaultOptions);

    public static T Deserialize<T>(string json)
        where T : notnull
        => JsonSerializer.Deserialize<T>(json, DefaultOptions)
           ?? throw new InvalidOperationException($"Failed to deserialize {typeof(T).Name} from JSON.");

    public static T? DeserializeOrDefault<T>(string json)
        where T : class
        => JsonSerializer.Deserialize<T>(json, DefaultOptions);
}
