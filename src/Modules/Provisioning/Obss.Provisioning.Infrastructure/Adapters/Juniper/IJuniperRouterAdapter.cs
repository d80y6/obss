using Obss.Provisioning.Infrastructure.Adapters.Common;
using Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;

namespace Obss.Provisioning.Infrastructure.Adapters.Juniper;

public interface IJuniperRouterAdapter
{
    string AdapterName { get; }

    // Interface Management
    Task<AdapterResult<InterfaceConfig>> ConfigureInterfaceAsync(InterfaceConfig config);
    Task<AdapterResult<InterfaceConfig>> GetInterfaceAsync(string interfaceName);
    Task<AdapterResult<IReadOnlyList<InterfaceStatus>>> GetInterfaceStatusesAsync();
    Task<AdapterResult> DeleteInterfaceAsync(string interfaceName);

    // Routing Protocols
    Task<AdapterResult<BgpConfig>> ConfigureBgpAsync(BgpConfig config);
    Task<AdapterResult<BgpConfig>> GetBgpConfigAsync();
    Task<AdapterResult<OspfConfig>> ConfigureOspfAsync(OspfConfig config);
    Task<AdapterResult> ConfigureStaticRouteAsync(StaticRoute route);
    Task<AdapterResult<IReadOnlyList<StaticRoute>>> GetStaticRoutesAsync();

    // System Configuration
    Task<AdapterResult<SystemConfig>> ConfigureSystemAsync(SystemConfig config);
    Task<AdapterResult<SystemConfig>> GetSystemConfigAsync();

    // Security (JunOS uses firewall filters)
    Task<AdapterResult<FirewallFilterConfig>> ConfigureFirewallFilterAsync(FirewallFilterConfig config);

    // Device Status
    Task<AdapterResult<DeviceStatus>> GetDeviceStatusAsync();
    Task<AdapterResult<DeviceInventory>> GetInventoryAsync();

    // Monitoring
    Task<AdapterResult<IReadOnlyList<AlarmInfo>>> GetActiveAlarmsAsync();

    // Health Check
    Task<AdapterResult<DeviceStatus>> HealthCheckAsync();
}
