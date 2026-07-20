using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Obss.Provisioning.Infrastructure.Adapters.ZTE;

public sealed class ZteAdapterHealthCheck : IHealthCheck
{
    private readonly IZteSoftswitchAdapter _adapter;
    private readonly ZteOperationProfile _profile;
    private readonly ILogger<ZteAdapterHealthCheck> _logger;

    public ZteAdapterHealthCheck(
        IZteSoftswitchAdapter adapter,
        ZteOperationProfile profile,
        ILogger<ZteAdapterHealthCheck> logger)
    {
        _adapter = adapter;
        _profile = profile;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionResult = await _adapter.TestConnectionAsync(cancellationToken);

            if (!connectionResult.IsSuccess)
            {
                _logger.LogWarning(
                    "ZTE adapter health check failed: {Error}",
                    connectionResult.ErrorMessage);

                return HealthCheckResult.Unhealthy(
                    description: $"ZTE softswitch connection failed: {connectionResult.ErrorMessage}",
                    data: GetHealthData());
            }

            var healthyData = GetHealthData();
            healthyData["connection_latency_ms"] = connectionResult.Data?.Latency.TotalMilliseconds.ToString("F0") ?? "unknown";
            healthyData["server_version"] = connectionResult.Data?.ServerVersion ?? "unknown";
            healthyData["protocol"] = connectionResult.Data?.Protocol ?? "unknown";

            return HealthCheckResult.Healthy(
                description: $"ZTE softswitch adapter is operational ({_adapter.AdapterName})",
                data: healthyData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ZTE adapter health check threw an exception");

            return HealthCheckResult.Unhealthy(
                description: $"ZTE adapter health check exception: {ex.Message}",
                exception: ex);
        }
    }

    private Dictionary<string, object> GetHealthData()
    {
        return new Dictionary<string, object>
        {
            ["adapter"] = _adapter.AdapterName,
            ["profile_version"] = _profile.ProfileVersion,
            ["vendor_product"] = _profile.VendorProduct,
            ["confirmed_operations"] = string.Join(", ", _profile.ConfirmedOperations),
            ["blocked_operations"] = string.Join(", ", _profile.BlockedOperations),
            ["confirmed_count"] = _profile.ConfirmedOperations.Count,
            ["blocked_count"] = _profile.BlockedOperations.Count,
        };
    }
}
