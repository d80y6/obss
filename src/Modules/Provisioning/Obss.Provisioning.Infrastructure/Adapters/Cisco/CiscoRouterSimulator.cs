using System.Collections.Concurrent;
using Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;
using Obss.Provisioning.Infrastructure.Adapters.Common;

namespace Obss.Provisioning.Infrastructure.Adapters.Cisco;

public sealed class CiscoRouterSimulator : ICiscoRouterAdapter
{
    public string AdapterName => CiscoAdapterConstants.AdapterName;

    private readonly ConcurrentDictionary<string, InterfaceConfig> _interfaces = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, StaticRoute> _staticRoutes = new(StringComparer.OrdinalIgnoreCase);
    private BgpConfig? _bgpConfig;
    private SystemConfig? _systemConfig;
    private readonly object _lock = new();

    public double FailureRate { get; set; } = 0.0;

    public void ClearState()
    {
        _interfaces.Clear();
        _staticRoutes.Clear();
        lock (_lock)
        {
            _bgpConfig = null;
            _systemConfig = null;
        }
    }

    private bool ShouldFail() => FailureRate > 0 && Random.Shared.NextDouble() < FailureRate;

    public Task<AdapterResult<InterfaceConfig>> ConfigureInterfaceAsync(InterfaceConfig config)
    {
        if (ShouldFail()) return Task.FromResult(AdapterResult<InterfaceConfig>.Failure("Simulated failure"));
        if (string.IsNullOrEmpty(config.Name))
            return Task.FromResult(AdapterResult<InterfaceConfig>.Failure("Interface name is required"));
        _interfaces[config.Name] = config;
        return Task.FromResult(AdapterResult<InterfaceConfig>.Success(config));
    }

    public Task<AdapterResult<InterfaceConfig>> GetInterfaceAsync(string interfaceName)
    {
        if (_interfaces.TryGetValue(interfaceName, out var config))
            return Task.FromResult(AdapterResult<InterfaceConfig>.Success(config));
        return Task.FromResult(AdapterResult<InterfaceConfig>.Failure($"Interface {interfaceName} not found"));
    }

    public Task<AdapterResult<IReadOnlyList<InterfaceStatus>>> GetInterfaceStatusesAsync()
    {
        var statuses = _interfaces.Values.Select(i => new InterfaceStatus(
            i.Name, i.AdminUp == true ? "up" : "down", "up", 1000000)).ToList();
        return Task.FromResult(AdapterResult<IReadOnlyList<InterfaceStatus>>.Success(statuses));
    }

    public Task<AdapterResult> DeleteInterfaceAsync(string interfaceName)
    {
        if (_interfaces.TryRemove(interfaceName, out _))
            return Task.FromResult(AdapterResult.Ok());
        return Task.FromResult(AdapterResult.Fail($"Interface {interfaceName} not found"));
    }

    public Task<AdapterResult<BgpConfig>> ConfigureBgpAsync(BgpConfig config)
    {
        if (ShouldFail()) return Task.FromResult(AdapterResult<BgpConfig>.Failure("Simulated failure"));
        lock (_lock) { _bgpConfig = config; }
        return Task.FromResult(AdapterResult<BgpConfig>.Success(config));
    }

    public Task<AdapterResult<BgpConfig>> GetBgpConfigAsync()
    {
        lock (_lock)
        {
            if (_bgpConfig is not null)
                return Task.FromResult(AdapterResult<BgpConfig>.Success(_bgpConfig));
        }
        return Task.FromResult(AdapterResult<BgpConfig>.Failure("BGP not configured"));
    }

    public Task<AdapterResult<OspfConfig>> ConfigureOspfAsync(OspfConfig config)
    {
        if (ShouldFail()) return Task.FromResult(AdapterResult<OspfConfig>.Failure("Simulated failure"));
        return Task.FromResult(AdapterResult<OspfConfig>.Success(config));
    }

    public Task<AdapterResult> ConfigureStaticRouteAsync(StaticRoute route)
    {
        if (ShouldFail()) return Task.FromResult(AdapterResult.Fail("Simulated failure"));
        if (string.IsNullOrEmpty(route.Prefix))
            return Task.FromResult(AdapterResult.Fail("Route prefix is required"));
        _staticRoutes[route.Prefix] = route;
        return Task.FromResult(AdapterResult.Ok());
    }

    public Task<AdapterResult<IReadOnlyList<StaticRoute>>> GetStaticRoutesAsync()
    {
        var routes = _staticRoutes.Values.ToList();
        return Task.FromResult(AdapterResult<IReadOnlyList<StaticRoute>>.Success(routes));
    }

    public Task<AdapterResult<SystemConfig>> ConfigureSystemAsync(SystemConfig config)
    {
        if (ShouldFail()) return Task.FromResult(AdapterResult<SystemConfig>.Failure("Simulated failure"));
        lock (_lock) { _systemConfig = config; }
        return Task.FromResult(AdapterResult<SystemConfig>.Success(config));
    }

    public Task<AdapterResult<SystemConfig>> GetSystemConfigAsync()
    {
        lock (_lock)
        {
            if (_systemConfig is not null)
                return Task.FromResult(AdapterResult<SystemConfig>.Success(_systemConfig));
        }
        return Task.FromResult(AdapterResult<SystemConfig>.Success(
            new SystemConfig("sim-router", "example.com", null, null, null)));
    }

    public Task<AdapterResult<AclConfig>> ConfigureAclAsync(AclConfig config)
    {
        if (ShouldFail()) return Task.FromResult(AdapterResult<AclConfig>.Failure("Simulated failure"));
        return Task.FromResult(AdapterResult<AclConfig>.Success(config));
    }

    public Task<AdapterResult<DeviceStatus>> GetDeviceStatusAsync()
    {
        var statuses = _interfaces.Values.Select(i => new InterfaceStatus(
            i.Name, i.AdminUp == true ? "up" : "down", "up", 1000000)).ToList();
        var status = new DeviceStatus("sim-router", "17.9.1", "ISR4451-X/K9",
            "100 days", 25.0, 50.0, statuses);
        return Task.FromResult(AdapterResult<DeviceStatus>.Success(status));
    }

    public Task<AdapterResult<DeviceInventory>> GetInventoryAsync()
    {
        var components = new[] { new HardwareComponent("Chassis", "ISR4451-X/K9", "OK") };
        var inventory = new DeviceInventory("ISR4451-X/K9", "SIM-SN-001", "17.9.1", "4GB", "16GB", components);
        return Task.FromResult(AdapterResult<DeviceInventory>.Success(inventory));
    }

    public Task<AdapterResult<DeviceStatus>> HealthCheckAsync()
    {
        return GetDeviceStatusAsync();
    }

    public Task<AdapterResult<IReadOnlyList<AlarmInfo>>> GetActiveAlarmsAsync()
    {
        return Task.FromResult(AdapterResult<IReadOnlyList<AlarmInfo>>.Success(Array.Empty<AlarmInfo>()));
    }
}
