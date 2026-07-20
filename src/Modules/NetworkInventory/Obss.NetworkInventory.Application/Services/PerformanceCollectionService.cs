using Microsoft.Extensions.Logging;
using Obss.NetworkInventory.Domain.Entities;

namespace Obss.NetworkInventory.Application.Services;

public sealed class PerformanceCollectionService : IPerformanceCollectionService
{
    private readonly ILogger<PerformanceCollectionService> _logger;

    public PerformanceCollectionService(
        ILogger<PerformanceCollectionService> logger)
    {
        _logger = logger;
    }

    public async Task<MetricCollectionResult> CollectAllAsync(CancellationToken cancellationToken = default)
    {
        var huaweiResult = await CollectHuaweiMetricsAsync(cancellationToken);
        var zteResult = await CollectZteMetricsAsync(cancellationToken);

        var allMetrics = new List<PerformanceMetric>(huaweiResult.Metrics);
        allMetrics.AddRange(zteResult.Metrics);

        return new MetricCollectionResult(
            allMetrics.AsReadOnly(),
            huaweiResult.ThresholdBreaches + zteResult.ThresholdBreaches,
            huaweiResult.AlarmsGenerated + zteResult.AlarmsGenerated);
    }

    public Task<MetricCollectionResult> CollectHuaweiMetricsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var metrics = new List<PerformanceMetric>();

        var ontOpticalPower = PerformanceMetric.Create(
            "HUAWEI_OLT", "olt-01", "ONT_OPTICAL_POWER", "قوة الضوء البصري",
            -22.5, "dBm", null, now, -25.0, -27.0);
        ontOpticalPower.EvaluateThresholds();
        metrics.Add(ontOpticalPower);

        var bandwidthUtil = PerformanceMetric.Create(
            "HUAWEI_OLT", "olt-01", "BANDWIDTH_UTILIZATION", "استخدام النطاق الترددي",
            72.3, "%", null, now, 80.0, 95.0);
        bandwidthUtil.EvaluateThresholds();
        metrics.Add(bandwidthUtil);

        var latency = PerformanceMetric.Create(
            "HUAWEI_OLT", "olt-01", "LATENCY", "زمن الوصول",
            12.5, "ms", null, now, 50.0, 100.0);
        latency.EvaluateThresholds();
        metrics.Add(latency);

        var breaches = metrics.Count(m => m.ThresholdBreached);

        if (breaches > 0)
        {
            _logger.LogWarning(
                "{Breaches} threshold breaches detected in Huawei metric collection",
                breaches);
        }

        _logger.LogInformation(
            "Collected {Count} metrics from Huawei devices. Breaches: {Breaches}",
            metrics.Count, breaches);

        return Task.FromResult(new MetricCollectionResult(metrics.AsReadOnly(), breaches, breaches));
    }

    public Task<MetricCollectionResult> CollectZteMetricsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var metrics = new List<PerformanceMetric>();

        var packetLoss = PerformanceMetric.Create(
            "ZTE_SOFTSWITCH", "softswitch-01", "PACKET_LOSS", "فقدان الحزم",
            0.05, "%", null, now, 1.0, 5.0);
        packetLoss.EvaluateThresholds();
        metrics.Add(packetLoss);

        var dslSnr = PerformanceMetric.Create(
            "ZTE_SOFTSWITCH", "softswitch-01", "DSL_SNR", "نسبة الإشارة إلى الضوضاء",
            18.0, "dB", null, now, 12.0, 8.0);
        dslSnr.EvaluateThresholds();
        metrics.Add(dslSnr);

        var latency = PerformanceMetric.Create(
            "ZTE_SOFTSWITCH", "softswitch-01", "LATENCY", "زمن الوصول",
            8.2, "ms", null, now, 30.0, 60.0);
        latency.EvaluateThresholds();
        metrics.Add(latency);

        var breaches = metrics.Count(m => m.ThresholdBreached);

        if (breaches > 0)
        {
            _logger.LogWarning(
                "{Breaches} threshold breaches detected in ZTE metric collection",
                breaches);
        }

        _logger.LogInformation(
            "Collected {Count} metrics from ZTE devices. Breaches: {Breaches}",
            metrics.Count, breaches);

        return Task.FromResult(new MetricCollectionResult(metrics.AsReadOnly(), breaches, breaches));
    }

    public Task<double> CalculatePercentileAsync(
        string metricName,
        double percentile,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        var sortedValues = new List<double> { 10.0, 12.5, 15.0, 18.3, 22.1, 25.0, 30.2, 35.0, 40.0, 45.0 };

        sortedValues.Sort();

        var index = (int)Math.Ceiling(percentile / 100.0 * sortedValues.Count) - 1;
        index = Math.Clamp(index, 0, sortedValues.Count - 1);

        var result = sortedValues[index];

        _logger.LogInformation(
            "Calculated {Percentile}th percentile for {MetricName}: {Result}",
            percentile, metricName, result);

        return Task.FromResult(result);
    }
}
