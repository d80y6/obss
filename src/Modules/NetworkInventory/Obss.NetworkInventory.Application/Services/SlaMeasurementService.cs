using Microsoft.Extensions.Logging;
using Obss.NetworkInventory.Domain.Entities;

namespace Obss.NetworkInventory.Application.Services;

public sealed class SlaMeasurementService : ISlaMeasurementService
{
    private readonly IPerformanceCollectionService _performanceCollectionService;
    private readonly ILogger<SlaMeasurementService> _logger;

    private static readonly Dictionary<string, double> DefaultSlaTargets = new()
    {
        ["FTTH"] = 99.5,
        ["DIA"] = 99.9,
        ["PRI"] = 99.99,
        ["ADSL"] = 99.0,
        ["LTE"] = 99.0
    };

    private static readonly Dictionary<string, decimal> CreditRates = new()
    {
        ["FTTH"] = 0.05m,
        ["DIA"] = 0.10m,
        ["PRI"] = 0.15m,
        ["ADSL"] = 0.03m,
        ["LTE"] = 0.03m
    };

    public SlaMeasurementService(
        IPerformanceCollectionService performanceCollectionService,
        ILogger<SlaMeasurementService> logger)
    {
        _performanceCollectionService = performanceCollectionService;
        _logger = logger;
    }

    public async Task<SlaComplianceResult> CalculateComplianceAsync(
        string serviceId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        var collection = await _performanceCollectionService.CollectAllAsync(cancellationToken);
        var serviceMetrics = collection.Metrics
            .Where(m => m.ServiceId == serviceId || m.ServiceId is null)
            .ToList();

        var total = serviceMetrics.Count;
        var breached = serviceMetrics.Count(m => m.ThresholdBreached);
        var compliance = total > 0
            ? (double)(total - breached) / total * 100.0
            : 100.0;

        _logger.LogInformation(
            "SLA compliance for {ServiceId}: {Compliance:F2}% ({Breached}/{Total} breached)",
            serviceId, compliance, breached, total);

        return new SlaComplianceResult(serviceId, compliance, total, breached, null);
    }

    public async Task<SlaReport> GenerateReportAsync(
        string serviceId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        var compliance = await CalculateComplianceAsync(serviceId, from, to, cancellationToken);
        var credits = await CalculateCreditsAsync(serviceId, from, to, cancellationToken);

        _logger.LogInformation(
            "SLA report generated for {ServiceId}: {Compliance:F2}% compliance, {Credits:C} credits",
            serviceId, compliance.CompliancePercentage, credits);

        return new SlaReport(
            serviceId,
            from,
            to,
            compliance.CompliancePercentage,
            credits,
            [compliance]);
    }

    public async Task<decimal> CalculateCreditsAsync(
        string serviceId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        var serviceType = ResolveServiceType(serviceId);

        var target = DefaultSlaTargets.GetValueOrDefault(serviceType, 99.0);
        var rate = CreditRates.GetValueOrDefault(serviceType, 0.05m);

        var collection = await _performanceCollectionService.CollectAllAsync(cancellationToken);
        var serviceMetrics = collection.Metrics
            .Where(m => m.ServiceId == serviceId || m.ServiceId is null)
            .Where(m => m.CollectedAt >= from && m.CollectedAt <= to)
            .ToList();

        var total = serviceMetrics.Count;
        var breached = serviceMetrics.Count(m => m.ThresholdBreached);
        var actualCompliance = total > 0
            ? (double)(total - breached) / total * 100.0
            : 100.0;

        if (actualCompliance >= target)
        {
            return 0m;
        }

        var shortfall = target - actualCompliance;
        var monthlyFee = 100.0m;
        var credits = monthlyFee * (decimal)shortfall / 100.0m * rate;

        _logger.LogInformation(
            "SLA credits for {ServiceId}: {Credits:C} (compliance: {Compliance:F2}%, target: {Target}%)",
            serviceId, credits, actualCompliance, target);

        return Math.Round(credits, 2);
    }

    private static string ResolveServiceType(string serviceId)
    {
        if (serviceId.StartsWith("FTTH-", StringComparison.OrdinalIgnoreCase))
            return "FTTH";
        if (serviceId.StartsWith("DIA-", StringComparison.OrdinalIgnoreCase))
            return "DIA";
        if (serviceId.StartsWith("PRI-", StringComparison.OrdinalIgnoreCase))
            return "PRI";
        if (serviceId.StartsWith("ADSL-", StringComparison.OrdinalIgnoreCase))
            return "ADSL";
        if (serviceId.StartsWith("LTE-", StringComparison.OrdinalIgnoreCase))
            return "LTE";

        return "FTTH";
    }
}
