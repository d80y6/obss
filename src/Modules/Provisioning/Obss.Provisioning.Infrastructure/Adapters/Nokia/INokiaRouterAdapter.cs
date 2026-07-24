using Obss.Provisioning.Infrastructure.Adapters.Common;
using Obss.Provisioning.Infrastructure.Adapters.Nokia.Models;

namespace Obss.Provisioning.Infrastructure.Adapters.Nokia;

public interface INokiaRouterAdapter
{
    string AdapterName { get; }

    // Interface Management
    Task<AdapterResult<InterfaceConfig>> ConfigureInterfaceAsync(InterfaceConfig config);
    Task<AdapterResult<InterfaceConfig>> GetInterfaceAsync(string portId);
    Task<AdapterResult<IReadOnlyList<InterfaceStatus>>> GetInterfaceStatusesAsync();
    Task<AdapterResult> DeleteInterfaceAsync(string portId);

    // Routing Protocols
    Task<AdapterResult<BgpConfig>> ConfigureBgpAsync(BgpConfig config);
    Task<AdapterResult<BgpConfig>> GetBgpConfigAsync();
    Task<AdapterResult<OspfConfig>> ConfigureOspfAsync(OspfConfig config);
    Task<AdapterResult> ConfigureStaticRouteAsync(StaticRoute route);
    Task<AdapterResult<IReadOnlyList<StaticRoute>>> GetStaticRoutesAsync();

    // System Configuration
    Task<AdapterResult<SystemConfig>> ConfigureSystemAsync(SystemConfig config);
    Task<AdapterResult<SystemConfig>> GetSystemConfigAsync();

    // Security (Nokia uses IP filters)
    Task<AdapterResult<IpFilterConfig>> ConfigureIpFilterAsync(IpFilterConfig config);

    // Device Status
    Task<AdapterResult<DeviceStatus>> GetDeviceStatusAsync();
    Task<AdapterResult<DeviceInventory>> GetInventoryAsync();

    // Monitoring
    Task<AdapterResult<IReadOnlyList<AlarmInfo>>> GetActiveAlarmsAsync();

    // Health Check
    Task<AdapterResult<DeviceStatus>> HealthCheckAsync();
}
