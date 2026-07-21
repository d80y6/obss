using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Infrastructure.Transports.Abstractions;

namespace Obss.Provisioning.Infrastructure.Transports;

public sealed class TransportHealthCheck : IHealthCheck
{
    private readonly ITransportFactory _transportFactory;
    private readonly IEnumerable<ITransportConfig> _transportConfigs;
    private readonly ILogger<TransportHealthCheck> _logger;

    public TransportHealthCheck(
        ITransportFactory transportFactory,
        IEnumerable<ITransportConfig> transportConfigs,
        ILogger<TransportHealthCheck> logger)
    {
        _transportFactory = transportFactory;
        _transportConfigs = transportConfigs;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var configs = _transportConfigs.ToList();

        if (configs.Count == 0)
        {
            return HealthCheckResult.Degraded("No network transports configured");
        }

        var results = new Dictionary<string, object>();
        var failed = 0;

        foreach (var config in configs)
        {
            try
            {
                var transport = _transportFactory.CreateTransport(config);
                var connectionResult = await transport.TestConnectionAsync(cancellationToken);

                results[$"{config.Protocol}_{config.Host}:{config.Port}"] = new
                {
                    config.Protocol,
                    config.Host,
                    config.Port,
                    connected = connectionResult.Success,
                    connectionResult.ResponseTime,
                    error = connectionResult.ErrorMessage
                };

                if (!connectionResult.Success)
                    failed++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transport health check failed for {Protocol} {Host}:{Port}",
                    config.Protocol, config.Host, config.Port);

                results[$"{config.Protocol}_{config.Host}:{config.Port}"] = new
                {
                    config.Protocol,
                    config.Host,
                    config.Port,
                    connected = false,
                    error = ex.Message
                };
                failed++;
            }
        }

        if (failed == 0)
            return HealthCheckResult.Healthy("All transports connected");

        if (failed < configs.Count)
            return HealthCheckResult.Degraded($"{failed}/{configs.Count} transports failed");

        return HealthCheckResult.Unhealthy("All transports failed");
    }
}
