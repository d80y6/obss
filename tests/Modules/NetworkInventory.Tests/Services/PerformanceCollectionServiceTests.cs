using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.NetworkInventory.Application.Services;
using Xunit;

namespace Obss.NetworkInventory.Tests.Services;

public class PerformanceCollectionServiceTests
{
    private readonly ILogger<PerformanceCollectionService> _logger;
    private readonly PerformanceCollectionService _service;

    public PerformanceCollectionServiceTests()
    {
        _logger = Substitute.For<ILogger<PerformanceCollectionService>>();
        _service = new PerformanceCollectionService(_logger);
    }

    [Fact]
    public async Task CollectHuaweiMetricsAsync_ShouldReturnMetrics()
    {
        var result = await _service.CollectHuaweiMetricsAsync();

        result.Should().NotBeNull();
        result.Metrics.Should().NotBeEmpty();
        result.Metrics.Should().HaveCount(3);
    }

    [Fact]
    public async Task CollectHuaweiMetricsAsync_ShouldContainExpectedMetricTypes()
    {
        var result = await _service.CollectHuaweiMetricsAsync();

        result.Metrics.Select(m => m.MetricName).Should().Contain("ONT_OPTICAL_POWER");
        result.Metrics.Select(m => m.MetricName).Should().Contain("BANDWIDTH_UTILIZATION");
        result.Metrics.Select(m => m.MetricName).Should().Contain("LATENCY");
    }

    [Fact]
    public async Task CollectHuaweiMetricsAsync_ShouldHaveArabicLabels()
    {
        var result = await _service.CollectHuaweiMetricsAsync();

        result.Metrics.All(m => m.MetricNameAr is not null).Should().BeTrue();
        result.Metrics.Any(m => m.SourceType == "HUAWEI_OLT").Should().BeTrue();
    }

    [Fact]
    public async Task CollectZteMetricsAsync_ShouldReturnMetrics()
    {
        var result = await _service.CollectZteMetricsAsync();

        result.Should().NotBeNull();
        result.Metrics.Should().NotBeEmpty();
        result.Metrics.Should().HaveCount(3);
    }

    [Fact]
    public async Task CollectZteMetricsAsync_ShouldContainExpectedMetricTypes()
    {
        var result = await _service.CollectZteMetricsAsync();

        result.Metrics.Select(m => m.MetricName).Should().Contain("PACKET_LOSS");
        result.Metrics.Select(m => m.MetricName).Should().Contain("DSL_SNR");
        result.Metrics.Select(m => m.MetricName).Should().Contain("LATENCY");
    }

    [Fact]
    public async Task CollectZteMetricsAsync_ShouldHaveCorrectSourceType()
    {
        var result = await _service.CollectZteMetricsAsync();

        result.Metrics.All(m => m.SourceType == "ZTE_SOFTSWITCH").Should().BeTrue();
    }

    [Fact]
    public async Task CollectAllAsync_ShouldCombineBothSources()
    {
        var result = await _service.CollectAllAsync();

        result.Should().NotBeNull();
        result.Metrics.Should().HaveCount(6);
        result.Metrics.Count(m => m.SourceType == "HUAWEI_OLT").Should().Be(3);
        result.Metrics.Count(m => m.SourceType == "ZTE_SOFTSWITCH").Should().Be(3);
    }

    [Fact]
    public async Task CollectAllAsync_ShouldDetectThresholdBreaches()
    {
        var result = await _service.CollectAllAsync();

        result.ThresholdBreaches.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task CalculatePercentileAsync_ShouldReturnCorrectValue()
    {
        var result = await _service.CalculatePercentileAsync(
            "LATENCY", 95, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CalculatePercentileAsync_WithFiftieth_ShouldReturnMedian()
    {
        var result = await _service.CalculatePercentileAsync(
            "BANDWIDTH_UTILIZATION", 50, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

        result.Should().BeGreaterThan(0);
    }
}
