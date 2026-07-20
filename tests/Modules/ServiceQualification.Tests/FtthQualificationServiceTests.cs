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

public class FtthQualificationServiceTests
{
    private readonly ICoverageAreaRepository _coverageRepo;
    private readonly INetworkElementRepository _networkElementRepo;
    private readonly IFtthQualificationService _service;

    public FtthQualificationServiceTests()
    {
        _coverageRepo = Substitute.For<ICoverageAreaRepository>();
        _networkElementRepo = Substitute.For<INetworkElementRepository>();
        _service = new FtthQualificationService(_coverageRepo, _networkElementRepo);
    }

    [Fact]
    public async Task QualifyAsync_WhenNoCoverage_ReturnsUnqualifiedWithAlternatives()
    {
        _coverageRepo.GetByAddressAsync(Arg.Any<GeographicAddress>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await _service.QualifyAsync(new FtthQualificationRequest(
            "123 Main St", "Riyadh", null, null, null, 100, "residential"), CancellationToken.None);

        result.IsQualified.Should().BeFalse();
        result.Explanation.Should().Contain("No fiber coverage");
        result.ExplanationAr.Should().NotBeNullOrWhiteSpace();
        result.Alternatives.Should().NotBeNull();
        result.AlternativesAr.Should().NotBeNull();
    }

    [Fact]
    public async Task QualifyAsync_WhenFtthNotInCoverage_ReturnsUnqualifiedWithAvailableTechs()
    {
        var coverageArea = new CoverageArea(Guid.NewGuid(), "Riyadh", null, null, null, "12345");
        coverageArea.AddService(CoverageService.Create("ADSL 50Mbps", 50, "ADSL", 150m, true));
        _coverageRepo.GetByAddressAsync(Arg.Any<GeographicAddress>(), Arg.Any<CancellationToken>())
            .Returns([coverageArea]);

        var result = await _service.QualifyAsync(new FtthQualificationRequest(
            "123 Main St", "Riyadh", null, null, null, 100, "residential"), CancellationToken.None);

        result.IsQualified.Should().BeFalse();
        result.Alternatives.Should().Contain(a => a.Contains("ADSL"));
    }

    [Fact]
    public async Task QualifyAsync_WhenFtthAvailableAndOltCapacityOk_ReturnsQualified()
    {
        var tenantId = TenantId.New();
        var coverageArea = new CoverageArea(Guid.NewGuid(), "Riyadh", null, null, null, "12345");
        coverageArea.AddService(CoverageService.Create("FTTH 200Mbps", 200, "FTTH", 300m, true));

        var olt = OLT.Create(tenantId, "OLT-RUH-01", "olt-ruh-01.example.com", "10.0.1.1",
            "Huawei", "MA5800", null, "Riyadh DC", 16, 64, 10);
        olt.AddPONPort(1, PONPortType.GPON, 64, 20);

        _coverageRepo.GetByAddressAsync(Arg.Any<GeographicAddress>(), Arg.Any<CancellationToken>())
            .Returns([coverageArea]);
        _networkElementRepo.GetFilteredAsync("OLT", "Active", null, 0, 100, Arg.Any<CancellationToken>())
            .Returns([olt]);

        var result = await _service.QualifyAsync(new FtthQualificationRequest(
            "123 Main St", "Riyadh", null, 24.7136, 46.6753, 200, "residential"), CancellationToken.None);

        result.IsQualified.Should().BeTrue();
        result.CoverageDetail.Should().NotBeNull();
        result.CoverageDetail!.FiberAtPremises.Should().BeTrue();
        result.CoverageDetail.OltCapacityAvailable.Should().BeTrue();
        result.CoverageDetail.PonPortAvailable.Should().BeTrue();
        result.CoverageDetail.NearestOltName.Should().Be("OLT-RUH-01");
        result.RequiredWork.Should().NotBeNull();
        result.RequiredWork!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task QualifyAsync_WhenOltCapacityExhausted_ReturnsUnqualifiedWithDetail()
    {
        var tenantId = TenantId.New();
        var coverageArea = new CoverageArea(Guid.NewGuid(), "Riyadh", null, null, null, "12345");
        coverageArea.AddService(CoverageService.Create("FTTH 100Mbps", 100, "FTTH", 250m, true));

        var olt = OLT.Create(tenantId, "OLT-RUH-02", "olt-ruh-02.example.com", "10.0.1.2",
            "Nokia", "FX-16", null, "Riyadh East", 16, 64, 10);
        for (int i = 1; i <= 16; i++)
        {
            var port = olt.AddPONPort(i, PONPortType.GPON, 64, 20);
            for (int j = 0; j < 64; j++)
                port.ConnectONT();
        }
        olt.UpdatePortUtilization();

        _coverageRepo.GetByAddressAsync(Arg.Any<GeographicAddress>(), Arg.Any<CancellationToken>())
            .Returns([coverageArea]);
        _networkElementRepo.GetFilteredAsync("OLT", "Active", null, 0, 100, Arg.Any<CancellationToken>())
            .Returns([olt]);

        var result = await _service.QualifyAsync(new FtthQualificationRequest(
            "456 Oak Ave", "Riyadh", null, 24.7000, 46.7000, 100, "residential"), CancellationToken.None);

        result.IsQualified.Should().BeFalse();
        result.CoverageDetail.Should().NotBeNull();
        result.CoverageDetail!.OltCapacityAvailable.Should().BeFalse();
        result.CapacityConflicts.Should().NotBeNull();
        result.CapacityConflicts!.Count.Should().BeGreaterThan(0);
    }
}
