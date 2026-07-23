using System.Text.Json;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Infrastructure.Transports.Abstractions;
using Obss.Provisioning.Infrastructure.Transports.Restconf.Model;

namespace Obss.Provisioning.Infrastructure.Transports.Restconf;

public sealed class RestconfTransport : IRestconfTransport, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly RestconfTransportConfig _config;
    private readonly RestconfContentNegotiator _contentNegotiator;
    private readonly RestconfErrorParser _errorParser;
    private readonly YangLibraryCache _yangCache;
    private readonly ILogger? _logger;

    public TransportProtocol Protocol => TransportProtocol.Restconf;
    public ITransportConfig Config => _config;

    public RestconfTransport(
        RestconfTransportConfig config,
        ILogger<RestconfTransport>? logger = null)
    {
        _config = config;
        var baseUri = new Uri(config.BaseUri.TrimEnd('/'), UriKind.Absolute);
        _httpClient = new HttpClient { BaseAddress = baseUri };
        _contentNegotiator = new RestconfContentNegotiator();
        _errorParser = new RestconfErrorParser();
        _yangCache = new YangLibraryCache();
        _logger = logger;

        _httpClient.DefaultRequestHeaders.Accept.ParseAdd(_contentNegotiator.GetAcceptHeader());
    }

    public async Task<RestconfResult> GetAsync(string path, RestconfQueryParams? query = null, CancellationToken ct = default)
        => await ExecuteAsync(() => _httpClient.GetAsync(path, ct), ct);

    public async Task<RestconfResult> PostAsync(string path, object? body = null, CancellationToken ct = default)
    {
        var content = SerializeBody(body);
        return await ExecuteAsync(() => _httpClient.PostAsync(path, content, ct), ct);
    }

    public async Task<RestconfResult> PutAsync(string path, object? body = null, CancellationToken ct = default)
    {
        var content = SerializeBody(body);
        return await ExecuteAsync(() => _httpClient.PutAsync(path, content, ct), ct);
    }

    public async Task<RestconfResult> PatchAsync(string path, object? body = null, CancellationToken ct = default)
    {
        var content = SerializeBody(body);
        var request = new HttpRequestMessage(HttpMethod.Patch, path) { Content = content };
        return await ExecuteAsync(() => _httpClient.SendAsync(request, ct), ct);
    }

    public async Task<RestconfResult> DeleteAsync(string path, CancellationToken ct = default)
        => await ExecuteAsync(() => _httpClient.DeleteAsync(path, ct), ct);

    public async Task<YangLibraryContent> GetYangLibraryAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync("modules-state/module", ct);
        response.EnsureSuccessStatusCode();
        var raw = await response.Content.ReadAsStringAsync(ct);

        var modules = ParseYangLibraryJson(raw);
        var content = new YangLibraryContent(
            Guid.NewGuid().ToString("N"), modules, DateTime.UtcNow);

        _yangCache.Update("default", content);
        return content;
    }

    public async Task<TransportConnectionResult> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("", cancellationToken);
            return new TransportConnectionResult
            {
                Success = response.IsSuccessStatusCode,
                ResponseTime = TimeSpan.Zero
            };
        }
        catch (Exception ex)
        {
            return new TransportConnectionResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task<RestconfResult> ExecuteAsync(Func<Task<HttpResponseMessage>> requestFunc, CancellationToken cancellationToken)
    {
        try
        {
            var response = await requestFunc();
            var rawData = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/yang-data+json";
                var parsedError = _errorParser.ParseError(rawData, contentType);
                return RestconfResult.Fail(parsedError?.ToString() ?? $"HTTP {(int)response.StatusCode}");
            }

            return RestconfResult.Ok(rawData, protocol: TransportProtocol.Restconf);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "RESTCONF request failed");
            return RestconfResult.Fail(ex.Message);
        }
    }

    private StringContent? SerializeBody(object? body)
    {
        if (body is null) return null;
        var json = JsonSerializer.Serialize(body);
        return new StringContent(json, System.Text.Encoding.UTF8, _contentNegotiator.GetContentType());
    }

    private static IReadOnlyList<YangModule> ParseYangLibraryJson(string raw)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            if (!root.TryGetProperty("ietf-yang-library:modules-state", out var state)
                || !state.TryGetProperty("module", out var modArr))
                return Array.Empty<YangModule>();

            var modules = new List<YangModule>();
            foreach (var mod in modArr.EnumerateArray())
            {
                modules.Add(new YangModule(
                    mod.GetProperty("name").GetString() ?? "",
                    null,
                    mod.GetProperty("namespace").GetString() ?? "",
                    null,
                    [],
                    []));
            }
            return modules;
        }
        catch
        {
            return Array.Empty<YangModule>();
        }
    }

    public void Dispose() => _httpClient.Dispose();
}
