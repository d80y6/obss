using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Provisioning.Infrastructure.Adapters.Common;
using Obss.Provisioning.Infrastructure.Adapters.Huawei;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters;

public class HuaweiBroadbandSimulatorTests
{
    private readonly ILogger<HuaweiBroadbandSimulator> _logger;
    private readonly HuaweiAdapterConfig _config;
    private readonly HuaweiBroadbandSimulator _sut;

    public HuaweiBroadbandSimulatorTests()
    {
        _logger = Substitute.For<ILogger<HuaweiBroadbandSimulator>>();
        _config = new HuaweiAdapterConfig
        {
            ControllerUrl = "https://huawei-controller:8443",
            Username = "admin",
            DeviceModel = "MA5800",
            TimeoutSeconds = 30,
        };
        _sut = new HuaweiBroadbandSimulator(_logger, _config);
    }

    [Fact]
    public void AdapterName_ShouldReturnHuaweiBroadband()
    {
        _sut.AdapterName.Should().Be("HuaweiBroadband");
    }

    [Fact]
    public void TechnologyType_ShouldReturnFtth()
    {
        _sut.TechnologyType.Should().Be("ftth");
    }

    [Fact]
    public async Task ActivateFtthAsync_WithValidRequest_ShouldReturnSimulatedSuccess()
    {
        var request = new ActivateFtthRequest(
            "SUB-001", "HWTC00ABCDEF01", "olt-01.example.com",
            "0/1", 100, "pppoe_user", "pppoe_pass", 500, 2000);

        var result = await _sut.ActivateFtthAsync(request);

        result.Success.Should().BeTrue();
        result.State.Should().Be(AdapterOperationState.Simulated);
        result.AdapterName.Should().Be("HuaweiBroadband");
        result.ResultData.Should().NotBeNull();
        result.Duration.Should().BePositive();
    }

    [Fact]
    public async Task ActivateFtthAsync_WithEmptySubscriberId_ShouldReturnFail()
    {
        var request = new ActivateFtthRequest(
            "", "HWTC00ABCDEF01", "olt-01.example.com",
            "0/1", 100, null, null, 500, 2000);

        var result = await _sut.ActivateFtthAsync(request);

        result.Success.Should().BeFalse();
        result.State.Should().Be(AdapterOperationState.Failed);
        result.ErrorMessage.Should().Contain("SubscriberId");
    }

    [Fact]
    public async Task ActivateFtthAsync_WithInvalidOntSerial_ShouldReturnFail()
    {
        var request = new ActivateFtthRequest(
            "SUB-001", "INVALID", "olt-01.example.com",
            "0/1", 100, null, null, 500, 2000);

        var result = await _sut.ActivateFtthAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("ONT serial");
    }

    [Fact]
    public async Task ActivateFtthAsync_WithInvalidPonPort_ShouldReturnFail()
    {
        var request = new ActivateFtthRequest(
            "SUB-001", "HWTC00ABCDEF01", "olt-01.example.com",
            "99/99", 100, null, null, 500, 2000);

        var result = await _sut.ActivateFtthAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("PON port");
    }

    [Fact]
    public async Task ActivateFtthAsync_WithInvalidVlanId_ShouldReturnFail()
    {
        var request = new ActivateFtthRequest(
            "SUB-001", "HWTC00ABCDEF01", "olt-01.example.com",
            "0/1", 9999, null, null, 500, 2000);

        var result = await _sut.ActivateFtthAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("VLAN");
    }

    [Fact]
    public async Task ActivateFtthAsync_WithInvalidBandwidth_ShouldReturnFail()
    {
        var request = new ActivateFtthRequest(
            "SUB-001", "HWTC00ABCDEF01", "olt-01.example.com",
            "0/1", 100, null, null, 0, 2000);

        var result = await _sut.ActivateFtthAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("bandwidth");
    }

    [Fact]
    public async Task ActivateAdslAsync_WithValidRequest_ShouldReturnSimulatedSuccess()
    {
        var request = new ActivateAdslRequest(
            "SUB-002", "dslam-01.example.com", "1/1/1",
            "ADSL2+_8M", 0, 35, "pppoe_user", "pppoe_pass");

        var result = await _sut.ActivateAdslAsync(request);

        result.Success.Should().BeTrue();
        result.State.Should().Be(AdapterOperationState.Simulated);
        result.ResultData.Should().NotBeNull();
    }

    [Fact]
    public async Task ActivateAdslAsync_WithInvalidVpi_ShouldReturnFail()
    {
        var request = new ActivateAdslRequest(
            "SUB-002", "dslam-01.example.com", "1/1/1",
            "ADSL2+_8M", 999, 35, null, null);

        var result = await _sut.ActivateAdslAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("VPI");
    }

    [Fact]
    public async Task Activate4GAsync_WithValidRequest_ShouldReturnSimulatedSuccess()
    {
        var request = new Activate4GRequest(
            "SUB-003", "310150123456789", "967712345678",
            "internet", "gold", "10GB", "default");

        var result = await _sut.Activate4GAsync(request);

        result.Success.Should().BeTrue();
        result.State.Should().Be(AdapterOperationState.Simulated);
        result.ResultData.Should().NotBeNull();
    }

    [Fact]
    public async Task Activate4GAsync_WithInvalidImsi_ShouldReturnFail()
    {
        var request = new Activate4GRequest(
            "SUB-003", "123", "967712345678",
            "internet", "gold", "10GB", "default");

        var result = await _sut.Activate4GAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("IMSI");
    }

    [Fact]
    public async Task Activate4GAsync_WithInvalidMsisdn_ShouldReturnFail()
    {
        var request = new Activate4GRequest(
            "SUB-003", "310150123456789", "abc",
            "internet", "gold", "10GB", "default");

        var result = await _sut.Activate4GAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("MSISDN");
    }

    [Fact]
    public async Task ActivateWiFiAsync_WithValidRequest_ShouldReturnSimulatedSuccess()
    {
        var request = new ActivateWiFiRequest(
            "SUB-004", "HomeWiFi", "securePass123",
            "WPA2", "5GHz", 10);

        var result = await _sut.ActivateWiFiAsync(request);

        result.Success.Should().BeTrue();
        result.State.Should().Be(AdapterOperationState.Simulated);
        result.ResultData.Should().NotBeNull();
    }

    [Fact]
    public async Task ActivateWiFiAsync_WithInvalidEncryption_ShouldReturnFail()
    {
        var request = new ActivateWiFiRequest(
            "SUB-004", "HomeWiFi", "securePass123",
            "WEP", "5GHz", 10);

        var result = await _sut.ActivateWiFiAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("encryption");
    }

    [Fact]
    public async Task ActivateWiFiAsync_WithShortPassphrase_ShouldReturnFail()
    {
        var request = new ActivateWiFiRequest(
            "SUB-004", "HomeWiFi", "short",
            "WPA2", "5GHz", 10);

        var result = await _sut.ActivateWiFiAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("passphrase");
    }

    [Fact]
    public async Task SuspendServiceAsync_WithValidRequest_ShouldReturnSimulatedSuccess()
    {
        var request = new SuspendRequest("SUB-005", "ftth", "Non-payment");

        var result = await _sut.SuspendServiceAsync(request);

        result.Success.Should().BeTrue();
        result.State.Should().Be(AdapterOperationState.Simulated);
    }

    [Fact]
    public async Task SuspendServiceAsync_WithoutReason_ShouldReturnFail()
    {
        var request = new SuspendRequest("SUB-005", "ftth", "");

        var result = await _sut.SuspendServiceAsync(request);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task ResumeServiceAsync_WithValidRequest_ShouldReturnSimulatedSuccess()
    {
        var request = new ResumeRequest("SUB-006", "ftth");

        var result = await _sut.ResumeServiceAsync(request);

        result.Success.Should().BeTrue();
        result.State.Should().Be(AdapterOperationState.Simulated);
    }

    [Fact]
    public async Task ChangeServiceAsync_WithValidRequest_ShouldReturnSimulatedSuccess()
    {
        var request = new ChangeServiceRequest("SUB-007", "ftth", 1000, null, null);

        var result = await _sut.ChangeServiceAsync(request);

        result.Success.Should().BeTrue();
        result.State.Should().Be(AdapterOperationState.Simulated);
    }

    [Fact]
    public async Task TerminateServiceAsync_WithNonImmediate_ShouldReturnSimulatedSuccess()
    {
        var request = new TerminateRequest("SUB-008", "ftth", "Customer request", false);

        var result = await _sut.TerminateServiceAsync(request);

        result.Success.Should().BeTrue();
        result.State.Should().Be(AdapterOperationState.Simulated);
    }

    [Fact]
    public async Task TerminateServiceAsync_WithImmediate_ShouldReturnBlocked()
    {
        var request = new TerminateRequest("SUB-008", "ftth", "Customer request", true);

        var result = await _sut.TerminateServiceAsync(request);

        result.Success.Should().BeFalse();
        result.State.Should().Be(AdapterOperationState.BlockedNeedsOperator);
    }

    [Fact]
    public async Task GetDeviceStatusAsync_WithValidRequest_ShouldReturnSimulatedSuccess()
    {
        var request = new DeviceStatusRequest("olt-01.example.com", "MA5800");

        var result = await _sut.GetDeviceStatusAsync(request);

        result.Success.Should().BeTrue();
        result.State.Should().Be(AdapterOperationState.Simulated);
        result.ResultData.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAlarmsAsync_WithValidRequest_ShouldReturnSimulatedSuccess()
    {
        var request = new AlarmQueryRequest("olt-01.example.com", "critical", null, null, null);

        var result = await _sut.GetAlarmsAsync(request);

        result.Success.Should().BeTrue();
        result.State.Should().Be(AdapterOperationState.Simulated);
    }

    [Fact]
    public async Task CollectPerformanceMetricsAsync_WithValidRequest_ShouldReturnSimulatedSuccess()
    {
        var request = new MetricsRequest("olt-01.example.com", "interface", "5m");

        var result = await _sut.CollectPerformanceMetricsAsync(request);

        result.Success.Should().BeTrue();
        result.State.Should().Be(AdapterOperationState.Simulated);
    }

    [Fact]
    public async Task CollectPerformanceMetricsAsync_WithInvalidInterval_ShouldReturnFail()
    {
        var request = new MetricsRequest("olt-01.example.com", "interface", "10s");

        var result = await _sut.CollectPerformanceMetricsAsync(request);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task ReconcileInventoryAsync_WithValidRequest_ShouldReturnSimulatedSuccess()
    {
        var request = new ReconcileRequest("olt-01.example.com", "ont");

        var result = await _sut.ReconcileInventoryAsync(request);

        result.Success.Should().BeTrue();
        result.State.Should().Be(AdapterOperationState.Simulated);
        result.ResultData.Should().NotBeNull();
    }
}
