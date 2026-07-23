using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.Provisioning.Infrastructure.Adapters.Cisco;
using Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;
using Obss.Provisioning.Infrastructure.Adapters.Common;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters.Cisco;

public sealed class CiscoProvisioningAdapterTests
{
    private readonly ICiscoRouterAdapter _innerMock;
    private readonly CiscoProvisioningAdapter _adapter;

    public CiscoProvisioningAdapterTests()
    {
        _innerMock = Substitute.For<ICiscoRouterAdapter>();
        var logger = Substitute.For<ILogger<CiscoProvisioningAdapter>>();
        _adapter = new CiscoProvisioningAdapter(_innerMock, logger);
    }

    [Fact]
    public async Task ExecuteAsync_ConfigureInterface_DelegatesToAdapter()
    {
        var config = JsonSerializer.SerializeToDocument(new
        {
            name = "GigabitEthernet0/0/0",
            type = "GigabitEthernet",
            description = "WAN Link"
        });
        var task = ProvisioningTask.Create(Guid.NewGuid(), 1, ProvisioningTaskType.RouterInterfaceConfig, configuration: config);
        _innerMock.ConfigureInterfaceAsync(Arg.Any<InterfaceConfig>())
            .Returns(AdapterResult<InterfaceConfig>.Success(new InterfaceConfig("GigabitEthernet0/0/0", "GigabitEthernet", null, null, null, null, null, null, null)));

        var result = await _adapter.ExecuteAsync(task, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _innerMock.Received(1).ConfigureInterfaceAsync(Arg.Any<InterfaceConfig>());
    }

    [Fact]
    public async Task ExecuteAsync_ConfigureBgp_DelegatesToAdapter()
    {
        var config = JsonSerializer.SerializeToDocument(new
        {
            asNumber = 65001,
            routerId = "10.0.0.1"
        });
        var task = ProvisioningTask.Create(Guid.NewGuid(), 1, ProvisioningTaskType.RouterBgpConfig, configuration: config);
        _innerMock.ConfigureBgpAsync(Arg.Any<BgpConfig>())
            .Returns(AdapterResult<BgpConfig>.Success(new BgpConfig(65001, "10.0.0.1", null, null)));

        var result = await _adapter.ExecuteAsync(task, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _innerMock.Received(1).ConfigureBgpAsync(Arg.Any<BgpConfig>());
    }

    [Fact]
    public async Task ExecuteAsync_ConfigureOspf_DelegatesToAdapter()
    {
        var config = JsonSerializer.SerializeToDocument(new
        {
            processId = 100,
            routerId = "10.0.0.1"
        });
        var task = ProvisioningTask.Create(Guid.NewGuid(), 1, ProvisioningTaskType.RouterOspfConfig, configuration: config);
        _innerMock.ConfigureOspfAsync(Arg.Any<OspfConfig>())
            .Returns(AdapterResult<OspfConfig>.Success(new OspfConfig(100, "10.0.0.1", null)));

        var result = await _adapter.ExecuteAsync(task, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _innerMock.Received(1).ConfigureOspfAsync(Arg.Any<OspfConfig>());
    }

    [Fact]
    public async Task ExecuteAsync_ConfigureStaticRoute_DelegatesToAdapter()
    {
        var config = JsonSerializer.SerializeToDocument(new
        {
            prefix = "0.0.0.0/0",
            nextHop = "10.0.0.1"
        });
        var task = ProvisioningTask.Create(Guid.NewGuid(), 1, ProvisioningTaskType.RouterStaticRouteConfig, configuration: config);
        _innerMock.ConfigureStaticRouteAsync(Arg.Any<StaticRoute>())
            .Returns(AdapterResult.Ok());

        var result = await _adapter.ExecuteAsync(task, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _innerMock.Received(1).ConfigureStaticRouteAsync(Arg.Any<StaticRoute>());
    }

    [Fact]
    public async Task ExecuteAsync_ConfigureSystem_DelegatesToAdapter()
    {
        var config = JsonSerializer.SerializeToDocument(new
        {
            hostname = "router01",
            domainName = "example.com"
        });
        var task = ProvisioningTask.Create(Guid.NewGuid(), 1, ProvisioningTaskType.RouterSystemConfig, configuration: config);
        _innerMock.ConfigureSystemAsync(Arg.Any<SystemConfig>())
            .Returns(AdapterResult<SystemConfig>.Success(new SystemConfig("router01", "example.com", null, null, null)));

        var result = await _adapter.ExecuteAsync(task, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _innerMock.Received(1).ConfigureSystemAsync(Arg.Any<SystemConfig>());
    }

    [Fact]
    public async Task ExecuteAsync_GetDeviceStatus_DelegatesToAdapter()
    {
        var task = ProvisioningTask.Create(Guid.NewGuid(), 1, ProvisioningTaskType.GetRouterStatus);
        _innerMock.GetDeviceStatusAsync()
            .Returns(AdapterResult<DeviceStatus>.Success(new DeviceStatus("rtr01", "17.9.1", "C9300", "100 days", 25.0, 50.0, Array.Empty<InterfaceStatus>())));

        var result = await _adapter.ExecuteAsync(task, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _innerMock.Received(1).GetDeviceStatusAsync();
    }

    [Fact]
    public async Task ExecuteAsync_GetRouterInventory_DelegatesToAdapter()
    {
        var task = ProvisioningTask.Create(Guid.NewGuid(), 1, ProvisioningTaskType.GetRouterInventory);
        _innerMock.GetInventoryAsync()
            .Returns(AdapterResult<DeviceInventory>.Success(new DeviceInventory("C9300", "SN123", "17.9.1", "4GB", "8GB", Array.Empty<HardwareComponent>())));

        var result = await _adapter.ExecuteAsync(task, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _innerMock.Received(1).GetInventoryAsync();
    }

    [Fact]
    public async Task ExecuteAsync_GetRouterAlarms_DelegatesToAdapter()
    {
        var task = ProvisioningTask.Create(Guid.NewGuid(), 1, ProvisioningTaskType.GetRouterAlarms);
        _innerMock.GetActiveAlarmsAsync()
            .Returns(AdapterResult<IReadOnlyList<AlarmInfo>>.Success(Array.Empty<AlarmInfo>()));

        var result = await _adapter.ExecuteAsync(task, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _innerMock.Received(1).GetActiveAlarmsAsync();
    }

    [Fact]
    public async Task ExecuteAsync_UnsupportedTask_ReturnsFailure()
    {
        var task = ProvisioningTask.Create(Guid.NewGuid(), 1, (ProvisioningTaskType)999);

        var result = await _adapter.ExecuteAsync(task, CancellationToken.None);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CompensateAsync_Interface_DelegatesToAdapter()
    {
        var config = JsonSerializer.SerializeToDocument(new
        {
            interfaceName = "GigabitEthernet0/0/0"
        });
        var task = ProvisioningTask.Create(Guid.NewGuid(), 1, ProvisioningTaskType.RouterInterfaceConfig, configuration: config);
        _innerMock.DeleteInterfaceAsync("GigabitEthernet0/0/0")
            .Returns(AdapterResult.Ok());

        var result = await _adapter.CompensateAsync(task, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _innerMock.Received(1).DeleteInterfaceAsync("GigabitEthernet0/0/0");
    }

    [Fact]
    public async Task CompensateAsync_UnsupportedTask_ReturnsOk()
    {
        var task = ProvisioningTask.Create(Guid.NewGuid(), 1, ProvisioningTaskType.GetRouterStatus);

        var result = await _adapter.CompensateAsync(task, CancellationToken.None);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_AdapterReturnsFailure_ReturnsFailure()
    {
        var task = ProvisioningTask.Create(Guid.NewGuid(), 1, ProvisioningTaskType.GetRouterStatus);
        _innerMock.GetDeviceStatusAsync()
            .Returns(AdapterResult<DeviceStatus>.Failure("Device unreachable"));

        var result = await _adapter.ExecuteAsync(task, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Device unreachable");
    }

    [Fact]
    public void AdapterName_IsCorrect()
    {
        _adapter.AdapterName.Should().Be(CiscoAdapterConstants.AdapterName);
    }
}
