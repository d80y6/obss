using System.Collections.Concurrent;
using Obss.Provisioning.Infrastructure.Adapters.Common;
using Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;

namespace Obss.Provisioning.Infrastructure.Adapters.Juniper;

public sealed class JuniperRouterSimulator : IJuniperRouterAdapter
{
    private readonly ConcurrentDictionary<string, InterfaceConfig> _interfaces = new();
    private readonly ConcurrentDictionary<string, object> _configStore = new();
    private readonly Random _random = new();

    public string AdapterName => JuniperAdapterConstants.AdapterName;

    public double FailureRate { get; set; }

    public void ClearState()
    {
        _interfaces.Clear();
        _configStore.Clear();
    }

    public Task<AdapterResult<InterfaceConfig>> ConfigureInterfaceAsync(InterfaceConfig config)
    {
        if (ShouldFail()) return Fail<InterfaceConfig>("Simulated failure");
        _interfaces[config.Name] = config;
        return Success(config);
    }

    public Task<AdapterResult<InterfaceConfig>> GetInterfaceAsync(string interfaceName)
    {
        if (ShouldFail()) return Fail<InterfaceConfig>("Simulated failure");
        return _interfaces.TryGetValue(interfaceName, out var config)
            ? Success(config)
            : Fail<InterfaceConfig>($"Interface {interfaceName} not found");
    }

    public Task<AdapterResult<IReadOnlyList<InterfaceStatus>>> GetInterfaceStatusesAsync()
    {
        if (ShouldFail()) return Fail<IReadOnlyList<InterfaceStatus>>("Simulated failure");
        var statuses = _interfaces.Values.Select(i => new InterfaceStatus(
            i.Name, "up", "up", 1000000)).ToList();
        return Success<IReadOnlyList<InterfaceStatus>>(statuses);
    }

    public Task<AdapterResult> DeleteInterfaceAsync(string interfaceName)
    {
        if (ShouldFail()) return Task.FromResult(AdapterResult.Fail("Simulated failure"));
        return _interfaces.TryRemove(interfaceName, out _)
            ? Task.FromResult(AdapterResult.Ok())
            : Task.FromResult(AdapterResult.Fail($"Interface {interfaceName} not found"));
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
            : Success(new SystemConfig("juniper-router", "example.com", null, null, null));
    }

    public Task<AdapterResult<FirewallFilterConfig>> ConfigureFirewallFilterAsync(FirewallFilterConfig config)
    {
        if (ShouldFail()) return Fail<FirewallFilterConfig>("Simulated failure");
        _configStore[$"filter:{config.Name}"] = config;
        return Success(config);
    }

    public Task<AdapterResult<DeviceStatus>> GetDeviceStatusAsync()
    {
        if (ShouldFail()) return Fail<DeviceStatus>("Simulated failure");
        var statuses = _interfaces.Values.Select(i => new InterfaceStatus(
            i.Name, "up", "up", 1000000)).ToList();
        return Success(new DeviceStatus("junos-router", "21.2R1", "MX204", "SN12345",
            3600, 4096, 15.5, statuses));
    }

    public Task<AdapterResult<DeviceInventory>> GetInventoryAsync()
    {
        if (ShouldFail()) return Fail<DeviceInventory>("Simulated failure");
        var components = new[]
        {
            new HardwareComponent("FPC0", "MX204-FPC", "SN-FPC-001", "Line Card", "Rev1", "750-12345"),
            new HardwareComponent("RE0", "MX204-RE", "SN-RE-001", "Routing Engine", "Rev2", "750-67890")
        };
        return Success(new DeviceInventory("MX204", "SN12345", "750-00001",
            "Juniper MX204 Router", "21.2R1", components));
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
