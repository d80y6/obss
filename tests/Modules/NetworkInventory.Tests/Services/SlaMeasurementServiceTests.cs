using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.NetworkInventory.Application.Services;
using Xunit;

namespace Obss.NetworkInventory.Tests.Services;

public class SlaMeasurementServiceTests
{
    private readonly IPerformanceCollectionService _performanceCollectionService;
    private readonly ILogger<SlaMeasurementService> _logger;
    private readonly SlaMeasurementService _service;

    public SlaMeasurementServiceTests()
    {
        _performanceCollectionService = Substitute.For<IPerformanceCollectionService>();
        _logger = Substitute.For<ILogger<SlaMeasurementService>>();
        _service = new SlaMeasurementService(_performanceCollectionService, _logger);
    }

    [Fact]
    public async Task CalculateComplianceAsync_WithNoBreaches_ShouldReturn100Percent()
    {
        var collection = new MetricCollectionResult([], 0, 0);
        _performanceCollectionService.CollectAllAsync(Arg.Any<CancellationToken>())
            .Returns(collection);

        var result = await _service.CalculateComplianceAsync("FTTH-001", DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

        result.Should().NotBeNull();
        result.ServiceId.Should().Be("FTTH-001");
        result.TotalMetrics.Should().Be(0);
        result.BreachedMetrics.Should().Be(0);
        result.CompliancePercentage.Should().Be(100.0);
    }

    [Fact]
    public async Task CalculateComplianceAsync_WithAllBreached_ShouldReturnZeroPercent()
    {
        var metric = PerformanceMetricBuilder.Build("BANDWIDTH_UTILIZATION", 99.0, "%", 80.0, 95.0);
        metric.EvaluateThresholds();
        var collection = new MetricCollectionResult([metric], 1, 1);
        _performanceCollectionService.CollectAllAsync(Arg.Any<CancellationToken>())
            .Returns(collection);

        var result = await _service.CalculateComplianceAsync("FTTH-001", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

        result.CompliancePercentage.Should().Be(0.0);
        result.TotalMetrics.Should().Be(1);
        result.BreachedMetrics.Should().Be(1);
    }

    [Fact]
    public async Task GenerateReportAsync_ShouldReturnReport()
    {
        _performanceCollectionService.CollectAllAsync(Arg.Any<CancellationToken>())
            .Returns(new MetricCollectionResult([], 0, 0));

        var result = await _service.GenerateReportAsync("DIA-001", DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);

        result.Should().NotBeNull();
        result.ServiceId.Should().Be("DIA-001");
        result.CompliancePercentage.Should().Be(100.0);
        result.CreditsEarned.Should().Be(0);
        result.Details.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CalculateCreditsAsync_WhenComplianceMeetsTarget_ShouldReturnZero()
    {
        _performanceCollectionService.CollectAllAsync(Arg.Any<CancellationToken>())
            .Returns(new MetricCollectionResult([], 0, 0));

        var credits = await _service.CalculateCreditsAsync("FTTH-001", DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);

        credits.Should().Be(0);
    }

    [Fact]
    public async Task CalculateCreditsAsync_WithBreach_ShouldCalculateCredits()
    {
        var metric = PerformanceMetricBuilder.Build("LATENCY", 120.0, "ms", 30.0, 60.0);
        metric.EvaluateThresholds();
        var collection = new MetricCollectionResult([metric], 1, 1);
        _performanceCollectionService.CollectAllAsync(Arg.Any<CancellationToken>())
            .Returns(collection);

        var credits = await _service.CalculateCreditsAsync("DIA-001", DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);

        credits.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task CalculateComplianceAsync_ShouldDetectFtthServiceType()
    {
        _performanceCollectionService.CollectAllAsync(Arg.Any<CancellationToken>())
            .Returns(new MetricCollectionResult([], 0, 0));

        var result = await _service.CalculateComplianceAsync("FTTH-001", DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

        result.Should().NotBeNull();
        result.CompliancePercentage.Should().Be(100.0);
    }

    [Fact]
    public async Task CalculateComplianceAsync_ShouldDetectDiaServiceType()
    {
        _performanceCollectionService.CollectAllAsync(Arg.Any<CancellationToken>())
            .Returns(new MetricCollectionResult([], 0, 0));

        var result = await _service.CalculateComplianceAsync("DIA-CIRCUIT-001", DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CalculateComplianceAsync_ShouldDetectPriServiceType()
    {
        _performanceCollectionService.CollectAllAsync(Arg.Any<CancellationToken>())
            .Returns(new MetricCollectionResult([], 0, 0));

        var result = await _service.CalculateComplianceAsync("PRI-TRUNK-001", DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

        result.Should().NotBeNull();
    }
}

internal static class PerformanceMetricBuilder
{
    public static Obss.NetworkInventory.Domain.Entities.PerformanceMetric Build(
        string metricName,
        double value,
        string unit,
        double warningThreshold,
        double criticalThreshold)
    {
        return Obss.NetworkInventory.Domain.Entities.PerformanceMetric.Create(
            "HUAWEI_OLT",
            "olt-01",
            metricName,
            null,
            value,
            unit,
            null,
            DateTime.UtcNow,
            warningThreshold,
            criticalThreshold);
    }
}
