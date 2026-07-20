using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Infrastructure.Adapters.Common;

namespace Obss.Provisioning.Infrastructure.Adapters.Huawei;

public sealed class HuaweiAdapterHealthCheck : IHealthCheck
{
    private readonly IDeviceConnectionManager _connectionManager;
    private readonly HuaweiAdapterConfig _config;
    private readonly ILogger<HuaweiAdapterHealthCheck> _logger;

    public HuaweiAdapterHealthCheck(
        IDeviceConnectionManager connectionManager,
        HuaweiAdapterConfig config,
        ILogger<HuaweiAdapterHealthCheck> logger)
    {
        _connectionManager = connectionManager;
        _config = config;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_config.ControllerUrl))
        {
            return HealthCheckResult.Degraded("Huawei adapter not configured - no controller URL set");
        }

        if (string.IsNullOrWhiteSpace(_config.Username) || string.IsNullOrWhiteSpace(_config.Password))
        {
            return HealthCheckResult.Degraded("Huawei adapter credentials not configured");
        }

        var connectionOk = await _connectionManager.TestConnectionAsync(
            _config.ControllerUrl,
            _config.TimeoutSeconds,
            cancellationToken);

        if (!connectionOk)
        {
            _logger.LogWarning("Huawei adapter health check: connection to {Url} failed", _config.ControllerUrl);
            return HealthCheckResult.Unhealthy($"Cannot reach Huawei controller at {_config.ControllerUrl}");
        }

        var credentialsOk = await _connectionManager.TestCredentialsAsync(
            _config.ControllerUrl,
            _config.Username,
            _config.Password,
            cancellationToken);

        if (!credentialsOk)
        {
            _logger.LogWarning("Huawei adapter health check: credential verification failed for {Url}", _config.ControllerUrl);
            return HealthCheckResult.Degraded($"Connected to {_config.ControllerUrl} but credential verification failed");
        }

        _logger.LogInformation("Huawei adapter health check: healthy");
        return HealthCheckResult.Healthy("Huawei adapter is operational");
    }
}
