using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Obss.Rating.Infrastructure.Services;

public sealed class HuaweiCounterCollector : IHuaweiCounterCollector
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HuaweiCounterCollector> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public HuaweiCounterCollector(
        HttpClient httpClient,
        ILogger<HuaweiCounterCollector> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CounterCollectionResult> CollectCountersAsync(
        string deviceAddress,
        string community,
        int port = 161,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Collecting Huawei counters via SNMP from {Device}:{Port} with community {Community}",
            deviceAddress, port, community);

        try
        {
            var url = $"http://{deviceAddress}:{port}/snmp/counters";
            var request = new { community, deviceAddress };
            var response = await _httpClient.PostAsJsonAsync(url, request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = $"SNMP counter collection failed: {response.StatusCode}";
                _logger.LogError(error);
                return new CounterCollectionResult([], [error]);
            }

            var counters = await response.Content.ReadFromJsonAsync<List<HuaweiCounterData>>(JsonOptions, cancellationToken);

            if (counters is null || counters.Count == 0)
            {
                _logger.LogWarning("No counters returned from device {Device}", deviceAddress);
                return new CounterCollectionResult([], ["No counters returned"]);
            }

            _logger.LogInformation(
                "Collected {Count} counters from device {Device} via SNMP",
                counters.Count, deviceAddress);

            return new CounterCollectionResult(counters.AsReadOnly(), []);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Device {Device} unreachable via SNMP", deviceAddress);
            return new CounterCollectionResult([], [$"Device {deviceAddress} unreachable: {ex.Message}"]);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "SNMP counter collection timed out for device {Device}", deviceAddress);
            return new CounterCollectionResult([], [$"Device {deviceAddress} timed out"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error collecting counters from {Device}", deviceAddress);
            return new CounterCollectionResult([], [$"Unexpected error: {ex.Message}"]);
        }
    }

    public async Task<CounterCollectionResult> CollectViaRestconfAsync(
        string deviceAddress,
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Collecting Huawei counters via RESTCONF from {Device}",
            deviceAddress);

        try
        {
            var baseUrl = $"https://{deviceAddress}/restconf/data/huawei-usage:usage-counters";
            var request = new HttpRequestMessage(HttpMethod.Get, baseUrl);
            var authBytes = System.Text.Encoding.UTF8.GetBytes($"{username}:{password}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(authBytes));
            request.Headers.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/yang-data+json"));

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(30));

            var response = await _httpClient.SendAsync(request, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var error = $"RESTCONF collection failed: {response.StatusCode}";
                _logger.LogError(error);
                return new CounterCollectionResult([], [error]);
            }

            var counters = await response.Content.ReadFromJsonAsync<List<HuaweiCounterData>>(JsonOptions, cts.Token);

            if (counters is null || counters.Count == 0)
            {
                _logger.LogWarning("No RESTCONF counters from device {Device}", deviceAddress);
                return new CounterCollectionResult([], ["No counters returned via RESTCONF"]);
            }

            _logger.LogInformation(
                "Collected {Count} counters from device {Device} via RESTCONF",
                counters.Count, deviceAddress);

            return new CounterCollectionResult(counters.AsReadOnly(), []);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Device {Device} unreachable via RESTCONF", deviceAddress);
            return new CounterCollectionResult([], [$"Device {deviceAddress} unreachable via RESTCONF: {ex.Message}"]);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "RESTCONF counter collection timed out for device {Device}", deviceAddress);
            return new CounterCollectionResult([], [$"Device {deviceAddress} timed out via RESTCONF"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in RESTCONF collection from {Device}", deviceAddress);
            return new CounterCollectionResult([], [$"Unexpected RESTCONF error: {ex.Message}"]);
        }
    }
}
