using System.Collections.Concurrent;
using Obss.Provisioning.Infrastructure.Adapters.Common;
using Obss.Provisioning.Infrastructure.Adapters.Nokia.Models;

namespace Obss.Provisioning.Infrastructure.Adapters.Nokia;

public sealed class NokiaRouterSimulator : INokiaRouterAdapter
{
    private readonly ConcurrentDictionary<string, InterfaceConfig> _interfaces = new();
    private readonly ConcurrentDictionary<string, object> _configStore = new();
    private readonly Random _random = new();

    public string AdapterName => NokiaAdapterConstants.AdapterName;

    public double FailureRate { get; set; }

    public void ClearState()
    {
        _interfaces.Clear();
        _configStore.Clear();
    }

    public Task<AdapterResult<InterfaceConfig>> ConfigureInterfaceAsync(InterfaceConfig config)
    {
        if (ShouldFail()) return Fail<InterfaceConfig>("Simulated failure");
        _interfaces[config.PortId] = config;
        return Success(config);
    }

    public Task<AdapterResult<InterfaceConfig>> GetInterfaceAsync(string portId)
    {
        if (ShouldFail()) return Fail<InterfaceConfig>("Simulated failure");
        return _interfaces.TryGetValue(portId, out var config)
            ? Success(config)
            : Fail<InterfaceConfig>($"Port {portId} not found");
    }

    public Task<AdapterResult<IReadOnlyList<InterfaceStatus>>> GetInterfaceStatusesAsync()
    {
        if (ShouldFail()) return Fail<IReadOnlyList<InterfaceStatus>>("Simulated failure");
        var statuses = _interfaces.Values.Select(i => new InterfaceStatus(
            i.PortId, "up", "up", 10000000000L)).ToList();
        return Success<IReadOnlyList<InterfaceStatus>>(statuses);
    }

    public Task<AdapterResult> DeleteInterfaceAsync(string portId)
    {
        if (ShouldFail()) return Task.FromResult(AdapterResult.Fail("Simulated failure"));
        return _interfaces.TryRemove(portId, out _)
            ? Task.FromResult(AdapterResult.Ok())
            : Task.FromResult(AdapterResult.Fail($"Port {portId} not found"));
    }

    public Task<AdapterResult<BgpConfig>> ConfigureBgpAsync(BgpConfig config)
    {
        if (ShouldFail()) return Fail<BgpConfig>("Simulated failure");
        _configStore["bgp"] = config;
        return Success(config);
    }

    public Task<AdapterResult<BgpConfig>> GetBgpConfigAsync()
    {
        if (ShouldFail()) return Fail<BgpConfig>("Simulated failure");
        return _configStore.TryGetValue("bgp", out var bgp) && bgp is BgpConfig bc
            ? Success(bc)
            : Success(new BgpConfig(65001, "10.0.0.1", Array.Empty<BgpGroup>()));
    }

    public Task<AdapterResult<OspfConfig>> ConfigureOspfAsync(OspfConfig config)
    {
        if (ShouldFail()) return Fail<OspfConfig>("Simulated failure");
        _configStore["ospf"] = config;
        return Success(config);
    }

    public Task<AdapterResult> ConfigureStaticRouteAsync(StaticRoute route)
    {
        if (ShouldFail()) return Task.FromResult(AdapterResult.Fail("Simulated failure"));
        _configStore[$"route:{route.Prefix}"] = route;
        return Task.FromResult(AdapterResult.Ok());
    }

    public Task<AdapterResult<IReadOnlyList<StaticRoute>>> GetStaticRoutesAsync()
    {
        if (ShouldFail()) return Fail<IReadOnlyList<StaticRoute>>("Simulated failure");
        var routes = _configStore
            .Where(kv => kv.Key.StartsWith("route:"))
            .Select(kv => kv.Value)
            .OfType<StaticRoute>()
            .ToList();
        return Success<IReadOnlyList<StaticRoute>>(routes);
    }

    public Task<AdapterResult<SystemConfig>> ConfigureSystemAsync(SystemConfig config)
    {
        if (ShouldFail()) return Fail<SystemConfig>("Simulated failure");
        _configStore["system"] = config;
        return Success(config);
    }

    public Task<AdapterResult<SystemConfig>> GetSystemConfigAsync()
    {
        if (ShouldFail()) return Fail<SystemConfig>("Simulated failure");
        return _configStore.TryGetValue("system", out var sys) && sys is SystemConfig sc
            ? Success(sc)
            : Success(new SystemConfig("nokia-router", "example.com", null, null, null));
    }

    public Task<AdapterResult<IpFilterConfig>> ConfigureIpFilterAsync(IpFilterConfig config)
    {
        if (ShouldFail()) return Fail<IpFilterConfig>("Simulated failure");
        _configStore[$"filter:{config.Name}"] = config;
        return Success(config);
    }

    public Task<AdapterResult<DeviceStatus>> GetDeviceStatusAsync()
    {
        if (ShouldFail()) return Fail<DeviceStatus>("Simulated failure");
        var statuses = _interfaces.Values.Select(i => new InterfaceStatus(
            i.PortId, "up", "up", 10000000000L)).ToList();
        return Success(new DeviceStatus("nokia-sros", "20.10.R1", "7750 SR-12", "SN12345",
            7200, 8192, 25.0, statuses));
    }

    public Task<AdapterResult<DeviceInventory>> GetInventoryAsync()
    {
        if (ShouldFail()) return Fail<DeviceInventory>("Simulated failure");
        var components = new[]
        {
            new ChassisComponent("1", "card", "SN-CARD-001", "3HE 12345", "IMM-12", "Rev1"),
            new ChassisComponent("1/1", "port", "SN-PORT-001", "3HE 67890", "10G SFP+", "Rev2")
        };
        return Success(new DeviceInventory("7750 SR-12", "SN12345", "3HE 00001",
            "Nokia 7750 SR Router", "20.10.R1", components));
    }

    public Task<AdapterResult<IReadOnlyList<AlarmInfo>>> GetActiveAlarmsAsync()
    {
        if (ShouldFail()) return Fail<IReadOnlyList<AlarmInfo>>("Simulated failure");
        return Success<IReadOnlyList<AlarmInfo>>(Array.Empty<AlarmInfo>());
    }

    public Task<AdapterResult<DeviceStatus>> HealthCheckAsync()
    {
        return GetDeviceStatusAsync();
    }

    private bool ShouldFail() => _random.NextDouble() < FailureRate;

    private static Task<AdapterResult<T>> Success<T>(T data) =>
        Task.FromResult(AdapterResult<T>.Success(data));

    private static Task<AdapterResult<T>> Fail<T>(string message) =>
        Task.FromResult(AdapterResult<T>.Failure(message));
}
