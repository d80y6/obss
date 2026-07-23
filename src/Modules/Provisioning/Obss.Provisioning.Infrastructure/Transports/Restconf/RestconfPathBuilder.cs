using System.Text.Encodings.Web;
using Obss.Provisioning.Infrastructure.Transports.Restconf.Model;

namespace Obss.Provisioning.Infrastructure.Transports.Restconf;

public sealed class RestconfPathBuilder
{
    private readonly string _baseUri;

    public RestconfPathBuilder(string baseUri)
    {
        _baseUri = baseUri.TrimEnd('/');
    }

    public string DataPath(string module, string container)
        => $"{_baseUri}/data/{module}:{container}";

    public string ListItemPath(string module, string list, string key)
        => $"{_baseUri}/data/{module}:{list}={UrlEncoder.Default.Encode(key)}";

    public string OperationPath(string module, string rpc)
        => $"{_baseUri}/operations/{module}:{rpc}";

    public static string WithQueryParams(string path, RestconfQueryParams? query)
    {
        if (query is null) return path;

        var parts = new List<string>();
        if (query.Depth.HasValue) parts.Add($"depth={query.Depth.Value}");
        if (!string.IsNullOrEmpty(query.Fields)) parts.Add($"fields={Uri.EscapeDataString(query.Fields)}");
        if (!string.IsNullOrEmpty(query.WithDefaults)) parts.Add($"with-defaults={query.WithDefaults}");

        return parts.Count > 0 ? $"{path}?{string.Join("&", parts)}" : path;
    }
}
