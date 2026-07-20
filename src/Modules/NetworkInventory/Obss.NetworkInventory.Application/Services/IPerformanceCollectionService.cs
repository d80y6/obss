using Obss.NetworkInventory.Domain.Entities;

namespace Obss.NetworkInventory.Application.Services;

public sealed record MetricCollectionResult(
    IReadOnlyList<PerformanceMetric> Metrics,
    int ThresholdBreaches,
    int AlarmsGenerated);

public interface IPerformanceCollectionService
{
    Task<MetricCollectionResult> CollectAllAsync(CancellationToken cancellationToken = default);
    Task<MetricCollectionResult> CollectHuaweiMetricsAsync(CancellationToken cancellationToken = default);
    Task<MetricCollectionResult> CollectZteMetricsAsync(CancellationToken cancellationToken = default);
    Task<double> CalculatePercentileAsync(string metricName, double percentile, DateTime from, DateTime to, CancellationToken cancellationToken = default);
}
