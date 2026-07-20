using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Infrastructure.Adapters.Common;
using Obss.Provisioning.Infrastructure.Adapters.Huawei;
using Obss.Provisioning.Infrastructure.Adapters.ZTE;

namespace Obss.Provisioning.Infrastructure.Health;

public sealed class AdapterHealthCheck : IHealthCheck
{
    private readonly AdapterRegistry _adapterRegistry;
    private readonly IDeviceConnectionManager _connectionManager;
    private readonly HuaweiAdapterConfig _huaweiConfig;
    private readonly ZteOperationProfile _zteProfile;
    private readonly ILogger<AdapterHealthCheck> _logger;

    public AdapterHealthCheck(
        AdapterRegistry adapterRegistry,
        IDeviceConnectionManager connectionManager,
        HuaweiAdapterConfig huaweiConfig,
        ZteOperationProfile zteProfile,
        ILogger<AdapterHealthCheck> logger)
    {
        _adapterRegistry = adapterRegistry;
        _connectionManager = connectionManager;
        _huaweiConfig = huaweiConfig;
        _zteProfile = zteProfile;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>();
        var issues = new List<string>();

        var allAdapters = _adapterRegistry.GetAllAdapters().ToList();
        data["registered_adapters"] = allAdapters.Count;

        if (allAdapters.Count == 0)
        {
            issues.Add("No adapters registered in registry");
            return HealthCheckResult.Degraded("No provision adapters registered", data: data);
        }

        data["adapters"] = string.Join(", ", allAdapters.Select(a => $"{a.Technology}/{a.Name}"));

        var simulators = allAdapters.Where(a => a.Name.Contains("Simulator", StringComparison.OrdinalIgnoreCase)).ToList();
        var realAdapters = allAdapters.Where(a => !a.Name.Contains("Simulator", StringComparison.OrdinalIgnoreCase)).ToList();
        data["simulator_count"] = simulators.Count;
        data["real_adapter_count"] = realAdapters.Count;

        foreach (var (tech, name, _) in allAdapters)
        {
            if (tech.Equals("huawei", StringComparison.OrdinalIgnoreCase))
            {
                var health = await CheckHuaweiAdapterAsync(cancellationToken);
                data[$"huawei_{name}_status"] = health.Status.ToString();
                data[$"huawei_{name}_description"] = health.Description ?? "";

                if (health.Status == HealthStatus.Unhealthy)
                {
                    issues.Add($"Huawei adapter '{name}' is unhealthy: {health.Description}");
                }
            }

            if (tech.Equals("zte", StringComparison.OrdinalIgnoreCase) && _zteProfile.BlockedOperations.Count > 0)
            {
                data[$"zte_{name}_blocked_operations"] = string.Join(", ", _zteProfile.BlockedOperations);
                issues.Add($"ZTE adapter '{name}' has {_zteProfile.BlockedOperations.Count} blocked operations");
            }
        }

        var huaweiConfigured = !string.IsNullOrWhiteSpace(_huaweiConfig.ControllerUrl);
        data["huawei_configured"] = huaweiConfigured;
        data["zte_blocked_operations"] = _zteProfile.BlockedOperations.Count;

        if (issues.Count > 0)
        {
            _logger.LogWarning("Adapter health check degraded: {Issues}", string.Join("; ", issues));
            return HealthCheckResult.Degraded(string.Join("; ", issues), data: data);
        }

        return HealthCheckResult.Healthy("All adapters operational", data: data);
    }

    private async Task<HealthCheckResult> CheckHuaweiAdapterAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_huaweiConfig.ControllerUrl))
        {
            return HealthCheckResult.Degraded("Huawei adapter not configured");
        }

        var connectionOk = await _connectionManager.TestConnectionAsync(
            _huaweiConfig.ControllerUrl,
            _huaweiConfig.TimeoutSeconds,
            cancellationToken);

        if (!connectionOk)
        {
            return HealthCheckResult.Unhealthy($"Cannot reach Huawei controller at {_huaweiConfig.ControllerUrl}");
        }

        return HealthCheckResult.Healthy("Huawei adapter reachable");
    }
}
