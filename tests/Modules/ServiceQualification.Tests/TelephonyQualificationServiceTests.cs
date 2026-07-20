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

public class TelephonyQualificationServiceTests
{
    private readonly ICoverageAreaRepository _coverageRepo;
    private readonly INetworkElementRepository _networkElementRepo;
    private readonly ITelephonyQualificationService _service;

    public TelephonyQualificationServiceTests()
    {
        _coverageRepo = Substitute.For<ICoverageAreaRepository>();
        _networkElementRepo = Substitute.For<INetworkElementRepository>();
        _service = new TelephonyQualificationService(_coverageRepo, _networkElementRepo);
    }

    [Fact]
    public async Task QualifyAsync_WhenInvalidPhoneNumber_ReturnsUnqualified()
    {
        var result = await _service.QualifyAsync(new TelephonyQualificationRequest(
            "123 Main St", "Riyadh", null, "invalid", "residential"), CancellationToken.None);

        result.IsQualified.Should().BeFalse();
        result.Explanation.Should().Contain("not valid");
        result.ExplanationAr.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task QualifyAsync_WhenNoCoverage_ReturnsUnqualifiedWithAlternatives()
    {
        _coverageRepo.GetByAddressAsync(Arg.Any<GeographicAddress>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await _service.QualifyAsync(new TelephonyQualificationRequest(
            "123 Main St", "Riyadh", null, "+966501234567", "residential"), CancellationToken.None);

        result.IsQualified.Should().BeFalse();
        result.Explanation.Should().Contain("No telephony coverage");
        result.ExplanationAr.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task QualifyAsync_WhenValidNumberAndCoverage_ReturnsQualified()
    {
        var coverageArea = new CoverageArea(Guid.NewGuid(), "Riyadh", null, null, null, "12345");
        coverageArea.AddService(CoverageService.Create("POTS", null, "POTS", 50m, true));

        var softswitch = NetworkElement.Create(TenantId.New(), "SSW-RUH-01", "ssw-ruh-01.example.com", "10.0.4.1",
            ElementType.Switch, "Genband", "C20", location: "Riyadh DC");

        _coverageRepo.GetByAddressAsync(Arg.Any<GeographicAddress>(), Arg.Any<CancellationToken>())
            .Returns([coverageArea]);
        _networkElementRepo.GetFilteredAsync("Softswitch", "Active", null, 0, 100, Arg.Any<CancellationToken>())
            .Returns([softswitch]);

        var result = await _service.QualifyAsync(new TelephonyQualificationRequest(
            "123 Main St", "Riyadh", null, "+966501234567", "residential"), CancellationToken.None);

        result.IsQualified.Should().BeTrue();
        result.CoverageDetail.Should().NotBeNull();
        result.CoverageDetail!.NumberAvailable.Should().BeTrue();
        result.CoverageDetail.SoftswitchCapacityAvailable.Should().BeTrue();
        result.RequiredWork.Should().NotBeNull();
        result.RequiredWork!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task QualifyAsync_WithSaudiFormat050_ReturnsQualified()
    {
        var coverageArea = new CoverageArea(Guid.NewGuid(), "Jeddah", null, null, null, "23456");
        coverageArea.AddService(CoverageService.Create("POTS", null, "POTS", 50m, true));

        var softswitch = NetworkElement.Create(TenantId.New(), "SSW-JED-01", "ssw-jed-01.example.com", "10.0.4.2",
            ElementType.Switch, "Genband", "C20", location: "Jeddah DC");

        _coverageRepo.GetByAddressAsync(Arg.Any<GeographicAddress>(), Arg.Any<CancellationToken>())
            .Returns([coverageArea]);
        _networkElementRepo.GetFilteredAsync("Softswitch", "Active", null, 0, 100, Arg.Any<CancellationToken>())
            .Returns([softswitch]);

        var result = await _service.QualifyAsync(new TelephonyQualificationRequest(
            "456 King Rd", "Jeddah", null, "0555123456", "residential"), CancellationToken.None);

        result.IsQualified.Should().BeTrue();
        result.CoverageDetail.Should().NotBeNull();
    }

    [Fact]
    public async Task QualifyAsync_WhenNoSoftswitch_ReturnsUnqualified()
    {
        var coverageArea = new CoverageArea(Guid.NewGuid(), "Riyadh", null, null, null, "12345");
        coverageArea.AddService(CoverageService.Create("POTS", null, "POTS", 50m, true));

        _coverageRepo.GetByAddressAsync(Arg.Any<GeographicAddress>(), Arg.Any<CancellationToken>())
            .Returns([coverageArea]);
        _networkElementRepo.GetFilteredAsync("Softswitch", "Active", null, 0, 100, Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await _service.QualifyAsync(new TelephonyQualificationRequest(
            "123 Main St", "Riyadh", null, "+966501234567", "residential"), CancellationToken.None);

        result.IsQualified.Should().BeFalse();
        result.CapacityConflicts.Should().NotBeNull();
        result.CapacityConflicts!.Count.Should().BeGreaterThan(0);
    }
}
