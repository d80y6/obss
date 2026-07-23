using System.Text.Json;

namespace Obss.Provisioning.Infrastructure.Transports.Restconf;

public sealed class RestconfContentNegotiator
{
    private const string JsonType = "application/yang-data+json";
    private const string XmlType = "application/yang-data+xml";
    private readonly bool _preferXml;

    public RestconfContentNegotiator(bool preferXml = false)
    {
        _preferXml = preferXml;
    }

    public string GetAcceptHeader(bool preferXml = false)
        => preferXml || _preferXml ? XmlType : JsonType;

    public string GetContentType()
        => _preferXml ? XmlType : JsonType;

    public static string Serialize(object? body, string contentType)
    {
        if (body is null) return string.Empty;

        if (contentType.Contains("xml"))
            return body is string s ? s : throw new NotSupportedException("XML body must be a string");

        return JsonSerializer.Serialize(body);
    }

    public static string? Deserialize(string? rawData, string contentType)
    {
        return rawData;
    }
}
