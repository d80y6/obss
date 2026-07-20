using System.Text.Json;
using FluentAssertions;
using NSubstitute;
using Obss.Orders.Application.Services;
using Xunit;

namespace Obss.Orders.Tests.Services;

public class DecompositionOrchestratorTests
{
    private readonly IFtthOrderDecompositionService _ftth;
    private readonly IAdslOrderDecompositionService _adsl;
    private readonly ILteOrderDecompositionService _lte;
    private readonly ITelephonyOrderDecompositionService _telephony;
    private readonly IBusinessConnectivityDecompositionService _business;
    private readonly IHostingDecompositionService _hosting;
    private readonly OrderDecompositionOrchestrator _orchestrator;

    public DecompositionOrchestratorTests()
    {
        _ftth = Substitute.For<IFtthOrderDecompositionService>();
        _adsl = Substitute.For<IAdslOrderDecompositionService>();
        _lte = Substitute.For<ILteOrderDecompositionService>();
        _telephony = Substitute.For<ITelephonyOrderDecompositionService>();
        _business = Substitute.For<IBusinessConnectivityDecompositionService>();
        _hosting = Substitute.For<IHostingDecompositionService>();

        _orchestrator = new OrderDecompositionOrchestrator(
            _ftth, _adsl, _lte, _telephony, _business, _hosting);
    }

    [Fact]
    public async Task DecomposeAsync_WithFtthServiceType_ShouldDelegateToFtthService()
    {
        var request = CreateRequest("FTTH");
        var ftthResult = new FtthDecompositionResult(
            Guid.NewGuid(),
            [new ServiceTask("FTTH_ONT_PROVISION", "Provision ONT", "توفير ONT", 1, null, null)],
            []);
        _ftth.DecomposeAsync(Arg.Any<FtthDecompositionRequest>(), Arg.Any<CancellationToken>())
            .Returns(ftthResult);

        var result = await _orchestrator.DecomposeAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.CorrelationId.Should().Be(ftthResult.CorrelationId);
        result.Tasks.Should().Contain(t => t.TaskType == "FTTH_ONT_PROVISION");
        await _ftth.Received(1).DecomposeAsync(Arg.Any<FtthDecompositionRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DecomposeAsync_WithAdslServiceType_ShouldDelegateToAdslService()
    {
        var request = CreateRequest("ADSL");
        var adslResult = new AdslDecompositionResult(
            Guid.NewGuid(),
            [new ServiceTask("ADSL_LINE_PROFILE_CONFIG", "Configure line", "تكوين خط", 1, null, null)],
            []);
        _adsl.DecomposeAsync(Arg.Any<AdslDecompositionRequest>(), Arg.Any<CancellationToken>())
            .Returns(adslResult);

        var result = await _orchestrator.DecomposeAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Tasks.Should().Contain(t => t.TaskType == "ADSL_LINE_PROFILE_CONFIG");
        await _adsl.Received(1).DecomposeAsync(Arg.Any<AdslDecompositionRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DecomposeAsync_WithLteServiceType_ShouldDelegateToLteService()
    {
        var request = CreateRequest("LTE");
        var lteResult = new LteDecompositionResult(
            Guid.NewGuid(),
            [new ServiceTask("LTE_APN_CONFIG", "APN config", "تكوين APN", 1, null, null)],
            []);
        _lte.DecomposeAsync(Arg.Any<LteDecompositionRequest>(), Arg.Any<CancellationToken>())
            .Returns(lteResult);

        var result = await _orchestrator.DecomposeAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Tasks.Should().Contain(t => t.TaskType == "LTE_APN_CONFIG");
        await _lte.Received(1).DecomposeAsync(Arg.Any<LteDecompositionRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DecomposeAsync_WithTelephonyServiceType_ShouldDelegateToTelephonyService()
    {
        var request = CreateRequest("TELEPHONY");
        var telResult = new TelephonyDecompositionResult(
            Guid.NewGuid(),
            [new ServiceTask("TEL_NUMBER_ACTIVATION", "Activate", "تفعيل", 1, null, null)],
            []);
        _telephony.DecomposeAsync(Arg.Any<TelephonyDecompositionRequest>(), Arg.Any<CancellationToken>())
            .Returns(telResult);

        var result = await _orchestrator.DecomposeAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Tasks.Should().Contain(t => t.TaskType == "TEL_NUMBER_ACTIVATION");
        await _telephony.Received(1).DecomposeAsync(Arg.Any<TelephonyDecompositionRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DecomposeAsync_WithDiaServiceType_ShouldDelegateToBusinessService()
    {
        var request = CreateRequest("DIA");
        var bizResult = new BusinessConnectivityDecompositionResult(
            Guid.NewGuid(),
            [new ServiceTask("BIZ_PORT_ALLOCATE", "Port", "منفذ", 1, null, null)],
            []);
        _business.DecomposeAsync(Arg.Any<BusinessConnectivityDecompositionRequest>(), Arg.Any<CancellationToken>())
            .Returns(bizResult);

        var result = await _orchestrator.DecomposeAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Tasks.Should().Contain(t => t.TaskType == "BIZ_PORT_ALLOCATE");
        await _business.Received(1).DecomposeAsync(Arg.Any<BusinessConnectivityDecompositionRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DecomposeAsync_WithDedicatedServerType_ShouldDelegateToHostingService()
    {
        var request = CreateRequest("DEDICATED_SERVER");
        var hstResult = new HostingDecompositionResult(
            Guid.NewGuid(),
            [new ServiceTask("HST_SERVER_PROVISION", "Server", "خادم", 1, null, null)],
            []);
        _hosting.DecomposeAsync(Arg.Any<HostingDecompositionRequest>(), Arg.Any<CancellationToken>())
            .Returns(hstResult);

        var result = await _orchestrator.DecomposeAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Tasks.Should().Contain(t => t.TaskType == "HST_SERVER_PROVISION");
        await _hosting.Received(1).DecomposeAsync(Arg.Any<HostingDecompositionRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DecomposeAsync_WithUnsupportedServiceType_ShouldThrow()
    {
        var request = CreateRequest("UNKNOWN");

        await FluentActions.Awaiting(() => _orchestrator.DecomposeAsync(request, CancellationToken.None))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DecomposeAsync_ShouldSetAdapterTypesCorrectly()
    {
        var request = CreateRequest("FTTH");
        var ftthResult = new FtthDecompositionResult(
            Guid.NewGuid(),
            [new ServiceTask("FTTH_ONT_PROVISION", "Provision ONT", "توفير ONT", 1, null, null)],
            []);
        _ftth.DecomposeAsync(Arg.Any<FtthDecompositionRequest>(), Arg.Any<CancellationToken>())
            .Returns(ftthResult);

        var result = await _orchestrator.DecomposeAsync(request, CancellationToken.None);

        result.Tasks.Should().Contain(t => t.AdapterType == "HUAWEI_BROADBAND");
    }

    [Fact]
    public async Task DecomposeAsync_ShouldReturnTasksInStepOrder()
    {
        var request = CreateRequest("FTTH");
        var ftthResult = new FtthDecompositionResult(
            Guid.NewGuid(),
            [
                new ServiceTask("T2", "Two", "اثنان", 2, "T1", null),
                new ServiceTask("T1", "One", "واحد", 1, null, null)
            ],
            []);
        _ftth.DecomposeAsync(Arg.Any<FtthDecompositionRequest>(), Arg.Any<CancellationToken>())
            .Returns(ftthResult);

        var result = await _orchestrator.DecomposeAsync(request, CancellationToken.None);

        result.Tasks.Select(t => t.StepOrder).Should().BeInAscendingOrder();
    }

    private static UnifiedDecompositionRequest CreateRequest(string serviceType)
    {
        return new UnifiedDecompositionRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            serviceType,
            "residential",
            null);
    }
}
