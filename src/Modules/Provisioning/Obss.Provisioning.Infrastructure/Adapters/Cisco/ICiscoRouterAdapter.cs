using Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;
using Obss.Provisioning.Infrastructure.Adapters.Common;

namespace Obss.Provisioning.Infrastructure.Adapters.Cisco;

public interface ICiscoRouterAdapter
{
    string AdapterName { get; }

    Task<AdapterResult<InterfaceConfig>> ConfigureInterfaceAsync(InterfaceConfig config);
    Task<AdapterResult<InterfaceConfig>> GetInterfaceAsync(string interfaceName);
    Task<AdapterResult<IReadOnlyList<InterfaceStatus>>> GetInterfaceStatusesAsync();
    Task<AdapterResult> DeleteInterfaceAsync(string interfaceName);

    Task<AdapterResult<BgpConfig>> ConfigureBgpAsync(BgpConfig config);
    Task<AdapterResult<BgpConfig>> GetBgpConfigAsync();
    Task<AdapterResult<OspfConfig>> ConfigureOspfAsync(OspfConfig config);
    Task<AdapterResult> ConfigureStaticRouteAsync(StaticRoute route);
    Task<AdapterResult<IReadOnlyList<StaticRoute>>> GetStaticRoutesAsync();

    Task<AdapterResult<SystemConfig>> ConfigureSystemAsync(SystemConfig config);
    Task<AdapterResult<SystemConfig>> GetSystemConfigAsync();

    Task<AdapterResult<AclConfig>> ConfigureAclAsync(AclConfig config);

    Task<AdapterResult<DeviceStatus>> GetDeviceStatusAsync();
    Task<AdapterResult<DeviceInventory>> GetInventoryAsync();

    Task<AdapterResult<IReadOnlyList<AlarmInfo>>> GetActiveAlarmsAsync();

    Task<AdapterResult<DeviceStatus>> HealthCheckAsync();
}
