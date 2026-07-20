using FluentAssertions;
using NSubstitute;
using Xunit;
using Obss.NetworkInventory.Application.Abstractions;
using Obss.NetworkInventory.Domain.Entities;
using Obss.NetworkInventory.Domain.ValueObjects;
using Obss.ServiceQualification.Domain.Abstractions;
using Obss.ServiceQualification.Domain.Entities;
using Obss.ServiceQualification.Domain.Services;
using Obss.ServiceQualification.Domain.ValueObjects;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.ServiceQualification.Tests;

public class LteQualificationServiceTests
{
    private readonly ICoverageAreaRepository _coverageRepo;
    private readonly INetworkElementRepository _networkElementRepo;
    private readonly ILteQualificationService _service;

    public LteQualificationServiceTests()
    {
        _coverageRepo = Substitute.For<ICoverageAreaRepository>();
        _networkElementRepo = Substitute.For<INetworkElementRepository>();
        _service = new LteQualificationService(_coverageRepo, _networkElementRepo);
    }

    [Fact]
    public async Task QualifyAsync_WhenNoCoverage_ReturnsUnqualifiedWithAlternatives()
    {
        _coverageRepo.GetByAddressAsync(Arg.Any<GeographicAddress>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await _service.QualifyAsync(new LteQualificationRequest(
            "123 Main St", "Riyadh", null, null, null, 50, "residential"), CancellationToken.None);

        result.IsQualified.Should().BeFalse();
        result.Explanation.Should().Contain("No LTE coverage");
        result.ExplanationAr.Should().NotBeNullOrWhiteSpace();
        result.Alternatives.Should().NotBeNull();
    }

    [Fact]
    public async Task QualifyAsync_WhenLteInCoverage_ReturnsQualified()
    {
        var coverageArea = new CoverageArea(Guid.NewGuid(), "Riyadh", null, null, null, "12345");
        coverageArea.AddService(CoverageService.Create("LTE 100Mbps", 100, "LTE", 200m, true));

        var enb = NetworkElement.Create(TenantId.New(), "ENB-RUH-01", "enb-ruh-01.example.com", "10.0.3.1",
            ElementType.AccessPoint, "Ericsson", "RBS 6501", location: "Riyadh Tower");

        _coverageRepo.GetByAddressAsync(Arg.Any<GeographicAddress>(), Arg.Any<CancellationToken>())
            .Returns([coverageArea]);
        _networkElementRepo.GetFilteredAsync("LTE_ENB", "Active", null, 0, 100, Arg.Any<CancellationToken>())
            .Returns([enb]);

        var result = await _service.QualifyAsync(new LteQualificationRequest(
            "123 Main St", "Riyadh", null, 24.7136, 46.6753, 50, "residential"), CancellationToken.None);

        result.IsQualified.Should().BeTrue();
        result.CoverageDetail.Should().NotBeNull();
        result.CoverageDetail!.CoverageAvailable.Should().BeTrue();
        result.CoverageDetail.CellCapacityAvailable.Should().BeTrue();
        result.CoverageDetail.EstimatedSignalStrength.Should().NotBeNull();
        result.CoverageDetail.EstimatedSpeed.Should().NotBeNull();
        result.RequiredWork.Should().NotBeNull();
        result.RequiredWork!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task QualifyAsync_WhenSpeedTooHigh_ReturnsUnqualified()
    {
        var coverageArea = new CoverageArea(Guid.NewGuid(), "Riyadh", null, null, null, "12345");
        coverageArea.AddService(CoverageService.Create("LTE 150Mbps", 150, "LTE", 200m, true));

        var enb = NetworkElement.Create(TenantId.New(), "ENB-RUH-02", "enb-ruh-02.example.com", "10.0.3.2",
            ElementType.AccessPoint, "Nokia", "Flexi Multiradio", location: "Riyadh North");

        _coverageRepo.GetByAddressAsync(Arg.Any<GeographicAddress>(), Arg.Any<CancellationToken>())
            .Returns([coverageArea]);
        _networkElementRepo.GetFilteredAsync("LTE_ENB", "Active", null, 0, 100, Arg.Any<CancellationToken>())
            .Returns([enb]);

        var result = await _service.QualifyAsync(new LteQualificationRequest(
            "789 Tower Rd", "Riyadh", null, 24.8000, 46.8000, 100, "residential"), CancellationToken.None);

        result.IsQualified.Should().BeFalse();
        result.CoverageDetail.Should().NotBeNull();
        result.CapacityConflicts.Should().NotBeNull();
        result.CapacityConflicts!.Count.Should().BeGreaterThan(0);
    }
}
