using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Infrastructure.Transports.Abstractions;

namespace Obss.Provisioning.Infrastructure.Transports.Rest;

public sealed class RestTransport : IRestTransport
{
    private readonly RestTransportConfig _config;
    private readonly HttpClient _httpClient;

    public RestTransport(RestTransportConfig config, ILogger<RestTransport> logger, IHttpClientFactory? httpClientFactory = null)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));

        _httpClient = httpClientFactory?.CreateClient("RestTransport") ?? CreateHttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);

        ConfigureAuthentication();
    }

    public TransportProtocol Protocol => TransportProtocol.Rest;
    public ITransportConfig Config => _config;

    public async Task<TransportConnectionResult> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var response = await _httpClient.GetAsync(_config.BaseUrl ?? $"http://{_config.Host}:{_config.Port}", cancellationToken);
            sw.Stop();

            return response.IsSuccessStatusCode
                ? TransportConnectionResult.Ok($"REST endpoint responded with {response.StatusCode}", sw.Elapsed)
                : TransportConnectionResult.Fail($"REST endpoint returned {response.StatusCode}", sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return TransportConnectionResult.Fail($"REST connection test failed: {ex.Message}", sw.Elapsed);
        }
    }

    public async Task<TransportResult> GetAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(async ct =>
        {
            var response = await _httpClient.GetAsync(BuildUrl(endpoint), ct);
            var content = await response.Content.ReadAsStringAsync(ct);
            return ProcessResponse(response, content);
        }, cancellationToken);
    }

    public async Task<TransportResult> PostAsync(string endpoint, object? body = null, CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(async ct =>
        {
            var response = await _httpClient.PostAsJsonAsync(BuildUrl(endpoint), body, ct);
            var content = await response.Content.ReadAsStringAsync(ct);
            return ProcessResponse(response, content);
        }, cancellationToken);
    }

    public async Task<TransportResult> PutAsync(string endpoint, object? body = null, CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(async ct =>
        {
            var response = await _httpClient.PutAsJsonAsync(BuildUrl(endpoint), body, ct);
            var content = await response.Content.ReadAsStringAsync(ct);
            return ProcessResponse(response, content);
        }, cancellationToken);
    }

    public async Task<TransportResult> PatchAsync(string endpoint, object? body = null, CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(async ct =>
        {
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Patch, BuildUrl(endpoint)) { Content = content };
            var response = await _httpClient.SendAsync(request, ct);
            var responseContent = await response.Content.ReadAsStringAsync(ct);
            return ProcessResponse(response, responseContent);
        }, cancellationToken);
    }

    public async Task<TransportResult> DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(async ct =>
        {
            var response = await _httpClient.DeleteAsync(BuildUrl(endpoint), ct);
            var content = await response.Content.ReadAsStringAsync(ct);
            return ProcessResponse(response, content);
        }, cancellationToken);
    }

    private async Task<TransportResult> ExecuteAsync(
        Func<CancellationToken, Task<(bool Success, string? Data, string? Error)>> operation,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var (success, data, error) = await operation(cancellationToken);
            sw.Stop();

            return success
                ? TransportResult.Ok(data, sw.Elapsed, Protocol)
                : TransportResult.Fail(error ?? "Request failed", sw.Elapsed, Protocol);
        }
        catch (TaskCanceledException)
        {
            sw.Stop();
            return TransportResult.Fail($"Request timed out after {_config.TimeoutSeconds}s", sw.Elapsed, Protocol);
        }
        catch (HttpRequestException ex)
        {
            sw.Stop();
            return TransportResult.Fail($"HTTP request failed: {ex.Message}", sw.Elapsed, Protocol);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return TransportResult.Fail($"Unexpected error: {ex.Message}", sw.Elapsed, Protocol);
        }
    }

    private static (bool Success, string? Data, string? Error) ProcessResponse(HttpResponseMessage response, string content)
    {
        return response.IsSuccessStatusCode
            ? (true, content, null)
            : (false, null, $"HTTP {(int)response.StatusCode}: {content}");
    }

    private string BuildUrl(string endpoint)
    {
        if (endpoint.StartsWith("http://") || endpoint.StartsWith("https://"))
            return endpoint;

        var baseUrl = _config.BaseUrl ?? $"{( _config.UseTls ? "https" : "http")}://{_config.Host}:{_config.Port}";
        baseUrl = baseUrl.TrimEnd('/');
        endpoint = endpoint.TrimStart('/');
        return $"{baseUrl}/{endpoint}";
    }

    private void ConfigureAuthentication()
    {
        switch (_config.AuthType)
        {
            case RestAuthType.Basic when !string.IsNullOrEmpty(_config.Username):
                var credentials = Convert.ToBase64String(
                    Encoding.ASCII.GetBytes($"{_config.Username}:{_config.Password ?? ""}"));
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                break;

            case RestAuthType.Bearer when !string.IsNullOrEmpty(_config.BearerToken):
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config.BearerToken);
                break;

            case RestAuthType.ApiKey when !string.IsNullOrEmpty(_config.ApiKey):
                var headerName = _config.ApiKeyHeader ?? "X-API-Key";
                _httpClient.DefaultRequestHeaders.Add(headerName, _config.ApiKey);
                break;
        }

        foreach (var (key, value) in _config.DefaultHeaders)
        {
            if (!_httpClient.DefaultRequestHeaders.Contains(key))
                _httpClient.DefaultRequestHeaders.Add(key, value);
        }
    }

    private static HttpClient CreateHttpClient()
    {
        return new HttpClient();
    }
}
