namespace Obss.SharedKernel.Application.Contracts;

public sealed record TmfResourceReference
{
    public string Id { get; init; } = string.Empty;
    public string Href { get; init; } = string.Empty;
    public string AtType { get; init; } = string.Empty;
    public string? SchemaLocation { get; init; }

    public static TmfResourceReference Create(string resourceName, Guid id, string? apiVersion = "v1")
    {
        var idStr = id.ToString();
        return new TmfResourceReference
        {
            Id = idStr,
            Href = $"/api/{apiVersion}/{resourceName}/{idStr}",
            AtType = ToPascalCase(resourceName),
            SchemaLocation = $"https://tmf-open-api.net/schemas/{resourceName}.json"
        };
    }

    public static TmfResourceReference Create(string resourceName, string id, string? apiVersion = "v1")
    {
        return new TmfResourceReference
        {
            Id = id,
            Href = $"/api/{apiVersion}/{resourceName}/{id}",
            AtType = ToPascalCase(resourceName),
            SchemaLocation = $"https://tmf-open-api.net/schemas/{resourceName}.json"
        };
    }

    private static string ToPascalCase(string s) =>
        string.Join("", s.Split('-', '_', '/').Select(w => char.ToUpper(w[0]) + w[1..]));
}
