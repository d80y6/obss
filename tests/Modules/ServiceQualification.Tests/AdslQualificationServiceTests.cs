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

public class AdslQualificationServiceTests
{
    private readonly ICoverageAreaRepository _coverageRepo;
    private readonly INetworkElementRepository _networkElementRepo;
    private readonly IAdslQualificationService _service;

    public AdslQualificationServiceTests()
    {
        _coverageRepo = Substitute.For<ICoverageAreaRepository>();
        _networkElementRepo = Substitute.For<INetworkElementRepository>();
        _service = new AdslQualificationService(_coverageRepo, _networkElementRepo);
    }

    [Fact]
    public async Task QualifyAsync_WhenNoCoverage_ReturnsUnqualifiedWithAlternatives()
    {
        _coverageRepo.GetByAddressAsync(Arg.Any<GeographicAddress>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await _service.QualifyAsync(new AdslQualificationRequest(
            "123 Main St", "Riyadh", null, null, null, 20, "residential"), CancellationToken.None);

        result.IsQualified.Should().BeFalse();
        result.Explanation.Should().Contain("No copper line coverage");
        result.ExplanationAr.Should().NotBeNullOrWhiteSpace();
        result.Alternatives.Should().NotBeNull();
        result.AlternativesAr.Should().NotBeNull();
    }

    [Fact]
    public async Task QualifyAsync_WhenAdslNotInCoverage_ReturnsUnqualifiedWithAvailableTechs()
    {
        var coverageArea = new CoverageArea(Guid.NewGuid(), "Riyadh", null, null, null, "12345");
        coverageArea.AddService(CoverageService.Create("FTTH 200Mbps", 200, "FTTH", 300m, true));
        _coverageRepo.GetByAddressAsync(Arg.Any<GeographicAddress>(), Arg.Any<CancellationToken>())
            .Returns([coverageArea]);

        var result = await _service.QualifyAsync(new AdslQualificationRequest(
            "123 Main St", "Riyadh", null, null, null, 20, "residential"), CancellationToken.None);

        result.IsQualified.Should().BeFalse();
        result.Alternatives.Should().Contain(a => a.Contains("FTTH"));
    }

    [Fact]
    public async Task QualifyAsync_WhenAdslAvailableAndWithinRange_ReturnsQualified()
    {
        var coverageArea = new CoverageArea(Guid.NewGuid(), "Riyadh", null, null, null, "12345");
        coverageArea.AddService(CoverageService.Create("ADSL 20Mbps", 20, "ADSL", 100m, true));

        var dslam = NetworkElement.Create(TenantId.New(), "DSLAM-RUH-01", "dslam-ruh-01.example.com", "10.0.2.1",
            ElementType.Switch, "Huawei", "MA5600T", location: "Riyadh East");
        dslam.AddInterface("adsl-1/1/1", null, InterfaceType.ADSL, 24, null, 1500);

        _coverageRepo.GetByAddressAsync(Arg.Any<GeographicAddress>(), Arg.Any<CancellationToken>())
            .Returns([coverageArea]);
        _networkElementRepo.GetFilteredAsync("DSLAM", "Active", null, 0, 100, Arg.Any<CancellationToken>())
            .Returns([dslam]);

        var result = await _service.QualifyAsync(new AdslQualificationRequest(
            "123 Main St", "Riyadh", null, 24.7136, 46.6753, 20, "residential"), CancellationToken.None);

        result.IsQualified.Should().BeTrue();
        result.CoverageDetail.Should().NotBeNull();
        result.CoverageDetail!.CopperLineAvailable.Should().BeTrue();
        result.CoverageDetail.DslamPortAvailable.Should().BeTrue();
        result.CoverageDetail.EstimatedSpeed.Should().NotBeNull();
        result.RequiredWork.Should().NotBeNull();
        result.RequiredWork!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task QualifyAsync_WhenSpeedBelowRequested_ReturnsUnqualified()
    {
        var coverageArea = new CoverageArea(Guid.NewGuid(), "Riyadh", null, null, null, "12345");
        coverageArea.AddService(CoverageService.Create("ADSL 24Mbps", 24, "ADSL", 100m, true));

        var dslam = NetworkElement.Create(TenantId.New(), "DSLAM-RUH-01", "dslam-ruh-01.example.com", "10.0.2.1",
            ElementType.Switch, "Huawei", "MA5600T", location: "Riyadh East");
        dslam.AddInterface("adsl-1/1/1", null, InterfaceType.ADSL, 24, null, 1500);

        _coverageRepo.GetByAddressAsync(Arg.Any<GeographicAddress>(), Arg.Any<CancellationToken>())
            .Returns([coverageArea]);
        _networkElementRepo.GetFilteredAsync("DSLAM", "Active", null, 0, 100, Arg.Any<CancellationToken>())
            .Returns([dslam]);

        var result = await _service.QualifyAsync(new AdslQualificationRequest(
            "456 Far Rd", "Riyadh", null, 24.8000, 46.8000, 24, "residential"), CancellationToken.None);

        result.IsQualified.Should().BeFalse();
        result.CoverageDetail.Should().NotBeNull();
        result.CapacityConflicts.Should().NotBeNull();
        result.CapacityConflicts!.Count.Should().BeGreaterThan(0);
    }
}
