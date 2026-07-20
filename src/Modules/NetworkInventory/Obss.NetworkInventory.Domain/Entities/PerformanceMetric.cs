using Obss.SharedKernel.Domain.Common;

namespace Obss.NetworkInventory.Domain.Entities;

public class PerformanceMetric : Entity<Guid>
{
    private PerformanceMetric() { }

    private PerformanceMetric(
        Guid id,
        string sourceType,
        string sourceName,
        string metricName,
        string? metricNameAr,
        double value,
        string unit,
        string? serviceId,
        DateTime collectedAt,
        double? warningThreshold,
        double? criticalThreshold)
        : base(id)
    {
        SourceType = sourceType;
        SourceName = sourceName;
        MetricName = metricName;
        MetricNameAr = metricNameAr;
        Value = value;
        Unit = unit;
        ServiceId = serviceId;
        CollectedAt = collectedAt;
        WarningThreshold = warningThreshold;
        CriticalThreshold = criticalThreshold;
        ThresholdBreached = false;
    }

    public string SourceType { get; private set; } = string.Empty;
    public string SourceName { get; private set; } = string.Empty;
    public string MetricName { get; private set; } = string.Empty;
    public string? MetricNameAr { get; private set; }
    public double Value { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public string? ServiceId { get; private set; }
    public DateTime CollectedAt { get; private set; }
    public double? WarningThreshold { get; private set; }
    public double? CriticalThreshold { get; private set; }
    public bool ThresholdBreached { get; private set; }

    public static PerformanceMetric Create(
        string sourceType,
        string sourceName,
        string metricName,
        string? metricNameAr,
        double value,
        string unit,
        string? serviceId,
        DateTime collectedAt,
        double? warningThreshold,
        double? criticalThreshold)
    {
        return new PerformanceMetric(
            Guid.NewGuid(),
            sourceType,
            sourceName,
            metricName,
            metricNameAr,
            value,
            unit,
            serviceId,
            collectedAt,
            warningThreshold,
            criticalThreshold);
    }

    public void EvaluateThresholds()
    {
        ThresholdBreached = false;

        if (CriticalThreshold.HasValue && Value > CriticalThreshold.Value)
        {
            ThresholdBreached = true;
            return;
        }

        if (WarningThreshold.HasValue && Value > WarningThreshold.Value)
        {
            ThresholdBreached = true;
        }
    }
}
