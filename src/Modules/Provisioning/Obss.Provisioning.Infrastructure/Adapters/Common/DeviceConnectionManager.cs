using Microsoft.Extensions.Logging;

namespace Obss.Provisioning.Infrastructure.Adapters.Common;

public sealed class DeviceConnectionManager : IDeviceConnectionManager
{
    private readonly ILogger<DeviceConnectionManager> _logger;
    private readonly HttpClient _httpClient;

    public DeviceConnectionManager(ILogger<DeviceConnectionManager> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<bool> TestConnectionAsync(string endpoint, int timeoutSeconds, CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            using var response = await _httpClient.SendAsync(request, cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Connection test to {Endpoint} failed", endpoint);
            return false;
        }
    }

    public async Task<bool> TestCredentialsAsync(string endpoint, string username, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            var authBytes = System.Text.Encoding.UTF8.GetBytes($"{username}:{password}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Credential test to {Endpoint} failed", endpoint);
            return false;
        }
    }
}
