using FluentAssertions;
using NSubstitute;
using Xunit;
using Obss.ServiceQualification.Domain.Services;

namespace Obss.ServiceQualification.Tests;

public class UnifiedQualificationServiceTests
{
    private readonly IFtthQualificationService _ftthService;
    private readonly IAdslQualificationService _adslService;
    private readonly ILteQualificationService _lteService;
    private readonly IWifiQualificationService _wifiService;
    private readonly ITelephonyQualificationService _telephonyService;
    private readonly IBusinessServiceQualificationService _businessService;
    private readonly QualificationEngine _engine;
    private readonly IUnifiedQualificationService _service;

    public UnifiedQualificationServiceTests()
    {
        _ftthService = Substitute.For<IFtthQualificationService>();
        _adslService = Substitute.For<IAdslQualificationService>();
        _lteService = Substitute.For<ILteQualificationService>();
        _wifiService = Substitute.For<IWifiQualificationService>();
        _telephonyService = Substitute.For<ITelephonyQualificationService>();
        _businessService = Substitute.For<IBusinessServiceQualificationService>();
        _engine = QualificationEngine.CreateDefault();
        _service = new UnifiedQualificationService(
            _ftthService, _adslService, _lteService, _wifiService,
            _telephonyService, _businessService, _engine);
    }

    [Fact]
    public async Task QualifyAsync_WithUnknownServiceType_ReturnsUnqualified()
    {
        var request = new UnifiedQualificationRequest(
            "123 Main St", "Riyadh", null, null, null,
            "UNKNOWN", "residential", null, null, null);

        var result = await _service.QualifyAsync(request, CancellationToken.None);

        result.IsQualified.Should().BeFalse();
        result.Explanation.Should().Contain("Unknown service type");
        result.ExplanationAr.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task QualifyAsync_WithFtthService_DelegatesToFtthService()
    {
        var ftthResult = new FtthQualificationResult(
            true, "corr-1",
            "Qualified for FTTH", "مؤهل للألياف",
            null, null, null, ["Fiber drop"], null);

        _ftthService.QualifyAsync(Arg.Any<FtthQualificationRequest>(), Arg.Any<CancellationToken>())
            .Returns(ftthResult);

        var request = new UnifiedQualificationRequest(
            "123 Main St", "Riyadh", null, 24.7136, 46.6753,
            "FTTH", "residential", 200, null, null);

        var result = await _service.QualifyAsync(request, CancellationToken.None);

        result.IsQualified.Should().BeTrue();
        result.ServiceType.Should().Be("FTTH");
        result.Explanation.Should().Contain("Qualified for FTTH");
        result.ExplanationAr.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task QualifyAsync_WithAdslService_DelegatesToAdslService()
    {
        var adslResult = new AdslQualificationResult(
            true, "corr-2",
            "Qualified for ADSL", "مؤهل لـ ADSL",
            null, null, null, ["Copper test"], null);

        _adslService.QualifyAsync(Arg.Any<AdslQualificationRequest>(), Arg.Any<CancellationToken>())
            .Returns(adslResult);

        var request = new UnifiedQualificationRequest(
            "123 Main St", "Riyadh", null, null, null,
            "ADSL", "residential", 20, null, null);

        var result = await _service.QualifyAsync(request, CancellationToken.None);

        result.IsQualified.Should().BeTrue();
        result.ServiceType.Should().Be("ADSL");
        result.RequiredWork.Should().Contain("Copper test");
    }

    [Fact]
    public async Task QualifyAsync_WithLteService_DelegatesToLteService()
    {
        var lteResult = new LteQualificationResult(
            true, "corr-3",
            "Qualified for LTE", "مؤهل لـ LTE",
            null, null, null, ["CPE install"], null);

        _lteService.QualifyAsync(Arg.Any<LteQualificationRequest>(), Arg.Any<CancellationToken>())
            .Returns(lteResult);

        var request = new UnifiedQualificationRequest(
            "123 Main St", "Riyadh", null, null, null,
            "LTE", "residential", 50, null, null);

        var result = await _service.QualifyAsync(request, CancellationToken.None);

        result.IsQualified.Should().BeTrue();
        result.ServiceType.Should().Be("LTE");
    }

    [Fact]
    public async Task QualifyAsync_WithTelephonyService_DelegatesToTelephonyService()
    {
        var telephonyResult = new TelephonyQualificationResult(
            true, "corr-4",
            "Qualified for Telephony", "مؤهل للهاتف",
            null, null, null, ["Line activation"], null);

        _telephonyService.QualifyAsync(Arg.Any<TelephonyQualificationRequest>(), Arg.Any<CancellationToken>())
            .Returns(telephonyResult);

        var request = new UnifiedQualificationRequest(
            "123 Main St", "Riyadh", null, null, null,
            "TELEPHONY", "residential", null, "+966501234567", null);

        var result = await _service.QualifyAsync(request, CancellationToken.None);

        result.IsQualified.Should().BeTrue();
        result.ServiceType.Should().Be("TELEPHONY");
    }

    [Fact]
    public async Task QualifyAsync_WithWifiService_DelegatesToWifiService()
    {
        var wifiResult = new WifiQualificationResult(
            true, "corr-5",
            "Qualified for WiFi", "مؤهل لواي فاي",
            null, null, null, ["CPE install"], null);

        _wifiService.QualifyAsync(Arg.Any<WifiQualificationRequest>(), Arg.Any<CancellationToken>())
            .Returns(wifiResult);

        var request = new UnifiedQualificationRequest(
            "123 Main St", "Riyadh", null, null, null,
            "WIFI", "residential", null, null, null);

        var result = await _service.QualifyAsync(request, CancellationToken.None);

        result.IsQualified.Should().BeTrue();
        result.ServiceType.Should().Be("WIFI");
    }

    [Fact]
    public async Task QualifyAsync_WithBusinessDia_DelegatesToBusinessService()
    {
        var businessResult = new BusinessServiceQualificationResult(
            true, "corr-6",
            "Qualified for DIA", "مؤهل لـ DIA",
            null, null, null, ["Fiber circuit"], null);

        _businessService.QualifyAsync(Arg.Any<BusinessServiceQualificationRequest>(), Arg.Any<CancellationToken>())
            .Returns(businessResult);

        var request = new UnifiedQualificationRequest(
            "123 Business Park", "Riyadh", null, null, null,
            "DIA", "business", 100, null, null);

        var result = await _service.QualifyAsync(request, CancellationToken.None);

        result.IsQualified.Should().BeTrue();
        result.ServiceType.Should().Be("DIA");
    }

    [Fact]
    public async Task QualifyAsync_WhenNotQualified_IncludesAlternatives()
    {
        var ftthResult = new FtthQualificationResult(
            false, "corr-7",
            "No fiber coverage", "لا توجد تغطية",
            ["ADSL", "LTE"], ["ADSL", "LTE"], null, null, null);

        _ftthService.QualifyAsync(Arg.Any<FtthQualificationRequest>(), Arg.Any<CancellationToken>())
            .Returns(ftthResult);

        var request = new UnifiedQualificationRequest(
            "456 Remote Rd", "Riyadh", null, null, null,
            "FTTH", "residential", 100, null, null);

        var result = await _service.QualifyAsync(request, CancellationToken.None);

        result.IsQualified.Should().BeFalse();
        result.Alternatives.Should().NotBeNull();
        result.Alternatives!.Count.Should().BeGreaterThan(0);
        result.AlternativesAr.Should().NotBeNull();
    }

    [Fact]
    public async Task QualifyAsync_RuleCheck_RejectsIneligibleRequest()
    {
        var request = new UnifiedQualificationRequest(
            "123 Main St", "Riyadh", null, null, null,
            "ADSL", "business", 100, null, null);

        var result = await _service.QualifyAsync(request, CancellationToken.None);

        result.IsQualified.Should().BeFalse();
        result.Explanation.Should().Contain("not eligible");
    }
}
