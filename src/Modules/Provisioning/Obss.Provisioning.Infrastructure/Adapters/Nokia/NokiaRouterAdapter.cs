using System.Text.Json;
using Obss.Provisioning.Infrastructure.Adapters.Common;
using Obss.Provisioning.Infrastructure.Adapters.Nokia.Models;
using Obss.Provisioning.Infrastructure.Transports.Restconf;

namespace Obss.Provisioning.Infrastructure.Adapters.Nokia;

public sealed class NokiaRouterAdapter : INokiaRouterAdapter, IDisposable
{
    private readonly IRestconfTransport _transport;

    public string AdapterName => NokiaAdapterConstants.AdapterName;

    public NokiaRouterAdapter(NokiaAdapterConfig config, IRestconfTransport transport)
    {
        _transport = transport;
    }

    public async Task<AdapterResult<InterfaceConfig>> ConfigureInterfaceAsync(InterfaceConfig config)
    {
        var path = $"{NokiaAdapterConstants.PortPath}/port-id={config.PortId}";
        var payload = new
        {
            port = new
            {
                portId = config.PortId,
                description = config.Description,
                adminState = config.AdminUp == true ? "enable" : "disable",
                mtu = config.Mtu,
                vlanTag = config.VlanTag,
                ip = config.IpAddress is not null ? new
                {
                    address = new
                    {
                        primary = new
                        {
                            address = config.IpAddress,
                            prefixLength = config.PrefixLength
                        }
                    }
                } : null
            }
        };
        var result = await _transport.PutAsync(path, payload);
        return result.Success
            ? AdapterResult<InterfaceConfig>.Success(config)
            : AdapterResult<InterfaceConfig>.Failure(result.ErrorMessage ?? "Unknown error");
    }

    public async Task<AdapterResult<InterfaceConfig>> GetInterfaceAsync(string portId)
    {
        var path = $"{NokiaAdapterConstants.PortPath}/port-id={portId}";
        var result = await _transport.GetAsync(path);
        if (!result.Success)
            return AdapterResult<InterfaceConfig>.Failure(result.ErrorMessage ?? "Unknown error");

        try
        {
            using var doc = JsonDocument.Parse(result.Data!);
            var config = ParseInterfaceConfig(doc.RootElement, portId);
            return config is not null
                ? AdapterResult<InterfaceConfig>.Success(config)
                : AdapterResult<InterfaceConfig>.Failure("Failed to parse interface");
        }
        catch
        {
            return AdapterResult<InterfaceConfig>.Failure("Failed to parse interface response");
        }
    }

    public async Task<AdapterResult<IReadOnlyList<InterfaceStatus>>> GetInterfaceStatusesAsync()
    {
        var result = await _transport.GetAsync(NokiaAdapterConstants.PortStatusPath);
        if (!result.Success)
            return AdapterResult<IReadOnlyList<InterfaceStatus>>.Failure(result.ErrorMessage ?? "Unknown error");

        try
        {
            using var doc = JsonDocument.Parse(result.Data!);
            var statuses = ParseInterfaceStatuses(doc.RootElement);
            return AdapterResult<IReadOnlyList<InterfaceStatus>>.Success(statuses);
        }
        catch
        {
            return AdapterResult<IReadOnlyList<InterfaceStatus>>.Failure("Failed to parse interface statuses");
        }
    }

    public async Task<AdapterResult> DeleteInterfaceAsync(string portId)
    {
        var path = $"{NokiaAdapterConstants.PortPath}/port-id={portId}";
        var result = await _transport.DeleteAsync(path);
        return result.Success
            ? AdapterResult.Ok()
            : AdapterResult.Fail(result.ErrorMessage ?? "Unknown error");
    }

    public async Task<AdapterResult<BgpConfig>> ConfigureBgpAsync(BgpConfig config)
    {
        var payload = new
        {
            bgp = new
            {
                group = config.Groups?.Select(g => new
                {
                    name = g.Name,
                    type = g.Type,
                    peerAs = g.PeerAs,
                    localAddress = g.LocalAddress,
                    neighbor = g.Neighbors?.Select(n => new
                    {
                        name = n.RemoteAddress,
                        peerAs = n.RemoteAs,
                        description = n.Description
                    }).ToArray()
                }).ToArray()
            }
        };
        var result = await _transport.PutAsync(NokiaAdapterConstants.BgpPath, payload);
        return result.Success
            ? AdapterResult<BgpConfig>.Success(config)
            : AdapterResult<BgpConfig>.Failure(result.ErrorMessage ?? "Unknown error");
    }

    public async Task<AdapterResult<BgpConfig>> GetBgpConfigAsync()
    {
        var result = await _transport.GetAsync(NokiaAdapterConstants.BgpPath);
        if (!result.Success)
            return AdapterResult<BgpConfig>.Failure(result.ErrorMessage ?? "Unknown error");
        try
        {
            using var doc = JsonDocument.Parse(result.Data!);
            var bgp = ParseBgpConfig(doc.RootElement);
            return bgp is not null
                ? AdapterResult<BgpConfig>.Success(bgp)
                : AdapterResult<BgpConfig>.Failure("Failed to parse BGP config");
        }
        catch
        {
            return AdapterResult<BgpConfig>.Failure("Failed to parse BGP config");
        }
    }

    public async Task<AdapterResult<OspfConfig>> ConfigureOspfAsync(OspfConfig config)
    {
        var payload = new
        {
            ospf = new
            {
                area = config.Areas?.Select(a => new
                {
                    areaId = a.AreaId,
                    @interface = a.Interfaces?.Select(i => new { name = i }).ToArray()
                }).ToArray()
            }
        };
        var result = await _transport.PutAsync(NokiaAdapterConstants.OspfPath, payload);
        return result.Success
            ? AdapterResult<OspfConfig>.Success(config)
            : AdapterResult<OspfConfig>.Failure(result.ErrorMessage ?? "Unknown error");
    }

    public async Task<AdapterResult> ConfigureStaticRouteAsync(StaticRoute route)
    {
        var payload = new
        {
            staticRoute = new[]
            {
                new
                {
                    prefix = route.Prefix,
                    nextHop = route.NextHop,
                    preference = route.Preference,
                    tag = route.Tag
                }
            }
        };
        var result = await _transport.PostAsync(NokiaAdapterConstants.StaticRoutePath, payload);
        return result.Success
            ? AdapterResult.Ok()
            : AdapterResult.Fail(result.ErrorMessage ?? "Unknown error");
    }

    public async Task<AdapterResult<IReadOnlyList<StaticRoute>>> GetStaticRoutesAsync()
    {
        var result = await _transport.GetAsync(NokiaAdapterConstants.StaticRoutePath);
        if (!result.Success)
            return AdapterResult<IReadOnlyList<StaticRoute>>.Failure(result.ErrorMessage ?? "Unknown error");
        try
        {
            using var doc = JsonDocument.Parse(result.Data!);
            var routes = ParseStaticRoutes(doc.RootElement);
            return AdapterResult<IReadOnlyList<StaticRoute>>.Success(routes);
        }
        catch
        {
            return AdapterResult<IReadOnlyList<StaticRoute>>.Failure("Failed to parse routes");
        }
    }

    public async Task<AdapterResult<SystemConfig>> ConfigureSystemAsync(SystemConfig config)
    {
        var payload = new
        {
            system = new
            {
                name = config.Name,
                domainName = config.DomainName,
                dns = config.DnsServers?.Any() == true ? new
                {
                    server = config.DnsServers.Select(d => new { address = d }).ToArray()
                } : null,
                ntp = config.NtpServers?.Any() == true ? new
                {
                    server = config.NtpServers.Select(n => new { address = n }).ToArray()
                } : null,
                snmp = config.SnmpLocation is not null ? new
                {
                    location = config.SnmpLocation
                } : null
            }
        };
        var result = await _transport.PutAsync(NokiaAdapterConstants.SystemPath, payload);
        return result.Success
            ? AdapterResult<SystemConfig>.Success(config)
            : AdapterResult<SystemConfig>.Failure(result.ErrorMessage ?? "Unknown error");
    }

    public async Task<AdapterResult<SystemConfig>> GetSystemConfigAsync()
    {
        var result = await _transport.GetAsync(NokiaAdapterConstants.SystemPath);
        if (!result.Success)
            return AdapterResult<SystemConfig>.Failure(result.ErrorMessage ?? "Unknown error");
        try
        {
            using var doc = JsonDocument.Parse(result.Data!);
            var sys = ParseSystemConfig(doc.RootElement);
            return sys is not null
                ? AdapterResult<SystemConfig>.Success(sys)
                : AdapterResult<SystemConfig>.Failure("Failed to parse system config");
        }
        catch
        {
            return AdapterResult<SystemConfig>.Failure("Failed to parse system config");
        }
    }

    public async Task<AdapterResult<IpFilterConfig>> ConfigureIpFilterAsync(IpFilterConfig config)
    {
        var payload = new
        {
            ipFilter = new
            {
                name = config.Name,
                description = config.Description,
                entry = config.Entries.Select(e => new
                {
                    sequence = e.Sequence,
                    action = e.Action,
                    sourceIp = e.SourceIp,
                    destinationIp = e.DestinationIp,
                    sourcePort = e.SourcePort,
                    destinationPort = e.DestinationPort,
                    protocol = e.Protocol,
                    log = e.Log
                }).ToArray()
            }
        };
        var result = await _transport.PutAsync($"{NokiaAdapterConstants.IpFilterPath}/name={config.Name}", payload);
        return result.Success
            ? AdapterResult<IpFilterConfig>.Success(config)
            : AdapterResult<IpFilterConfig>.Failure(result.ErrorMessage ?? "Unknown error");
    }

    public async Task<AdapterResult<DeviceStatus>> GetDeviceStatusAsync()
    {
        var result = await _transport.GetAsync(NokiaAdapterConstants.DeviceStatusPath);
        if (!result.Success)
            return AdapterResult<DeviceStatus>.Failure(result.ErrorMessage ?? "Unknown error");
        try
        {
            using var doc = JsonDocument.Parse(result.Data!);
            var status = ParseDeviceStatus(doc.RootElement);
            return status is not null
                ? AdapterResult<DeviceStatus>.Success(status)
                : AdapterResult<DeviceStatus>.Failure("Failed to parse device status");
        }
        catch
        {
            return AdapterResult<DeviceStatus>.Failure("Failed to parse device status");
        }
    }

    public async Task<AdapterResult<DeviceInventory>> GetInventoryAsync()
    {
        var result = await _transport.GetAsync(NokiaAdapterConstants.InventoryPath);
        if (!result.Success)
            return AdapterResult<DeviceInventory>.Failure(result.ErrorMessage ?? "Unknown error");
        try
        {
            using var doc = JsonDocument.Parse(result.Data!);
            var inventory = ParseInventory(doc.RootElement);
            return inventory is not null
                ? AdapterResult<DeviceInventory>.Success(inventory)
                : AdapterResult<DeviceInventory>.Failure("Failed to parse inventory");
        }
        catch
        {
            return AdapterResult<DeviceInventory>.Failure("Failed to parse inventory");
        }
    }

    public async Task<AdapterResult<IReadOnlyList<AlarmInfo>>> GetActiveAlarmsAsync()
    {
        var result = await _transport.GetAsync(NokiaAdapterConstants.AlarmsPath);
        if (!result.Success)
            return AdapterResult<IReadOnlyList<AlarmInfo>>.Failure(result.ErrorMessage ?? "Unknown error");
        try
        {
            using var doc = JsonDocument.Parse(result.Data!);
            var alarms = ParseAlarms(doc.RootElement);
            return AdapterResult<IReadOnlyList<AlarmInfo>>.Success(alarms);
        }
        catch
        {
            return AdapterResult<IReadOnlyList<AlarmInfo>>.Failure("Failed to parse alarms");
        }
    }

    public async Task<AdapterResult<DeviceStatus>> HealthCheckAsync()
    {
        var result = await _transport.GetAsync(NokiaAdapterConstants.DeviceStatusPath);
        if (!result.Success)
            return AdapterResult<DeviceStatus>.Failure(result.ErrorMessage ?? "Unknown error");
        return await GetDeviceStatusAsync();
    }

    public void Dispose()
    {
    }

    private static InterfaceConfig? ParseInterfaceConfig(JsonElement root, string portId)
    {
        if (!root.TryGetProperty("port", out var port))
            return null;
        var desc = port.TryGetProperty("description", out var d) ? d.GetString() : null;
        var mtu = port.TryGetProperty("mtu", out var m) && m.TryGetInt32(out var mtuVal) ? mtuVal : (int?)null;
        var vlanTag = port.TryGetProperty("vlanTag", out var v) ? v.GetString() : null;
        return new InterfaceConfig(portId, desc ?? "", null, mtu, vlanTag, null, null);
    }

    private static List<InterfaceStatus> ParseInterfaceStatuses(JsonElement root)
    {
        var statuses = new List<InterfaceStatus>();
        if (!root.TryGetProperty("port", out var portArr))
            return statuses;
        foreach (var p in portArr.EnumerateArray())
        {
            var portId = p.TryGetProperty("portId", out var id) ? id.GetString() : "unknown";
            var oper = p.TryGetProperty("operState", out var os) ? os.GetString() : "unknown";
            var admin = p.TryGetProperty("adminState", out var ad) ? ad.GetString() : "unknown";
            var speed = p.TryGetProperty("speed", out var s) && long.TryParse(s.GetString(), out var spd) ? spd : 0L;
            statuses.Add(new InterfaceStatus(portId ?? "unknown", oper ?? "unknown", admin ?? "unknown", speed));
        }
        return statuses;
    }

    private static BgpConfig? ParseBgpConfig(JsonElement root)
    {
        if (!root.TryGetProperty("bgp", out var bgp))
            return null;
        var groups = new List<BgpGroup>();
        if (bgp.TryGetProperty("group", out var grpEl))
        {
            foreach (var g in grpEl.EnumerateArray())
            {
                var name = g.TryGetProperty("name", out var gn) ? gn.GetString() : "";
                var type = g.TryGetProperty("type", out var gt) ? gt.GetString() : null;
                var peerAs = g.TryGetProperty("peerAs", out var pa) && pa.TryGetInt32(out var paVal) ? paVal : (int?)null;
                var localAddr = g.TryGetProperty("localAddress", out var la) ? la.GetString() : null;

                var neighbors = new List<BgpNeighbor>();
                if (g.TryGetProperty("neighbor", out var neighEl))
                {
                    foreach (var neigh in neighEl.EnumerateArray())
                    {
                        var addr = neigh.TryGetProperty("name", out var nn) ? nn.GetString() : "";
                        var rAs = neigh.TryGetProperty("peerAs", out var rp) && rp.TryGetInt32(out var rpVal) ? rpVal : (int?)null;
                        var desc = neigh.TryGetProperty("description", out var nd) ? nd.GetString() : null;
                        neighbors.Add(new BgpNeighbor(addr ?? "", rAs, null, desc));
                    }
                }
                groups.Add(new BgpGroup(name ?? "", type, peerAs, localAddr, neighbors));
            }
        }
        return new BgpConfig(0, null, groups);
    }

    private static List<StaticRoute> ParseStaticRoutes(JsonElement root)
    {
        var routes = new List<StaticRoute>();
        if (!root.TryGetProperty("staticRoute", out var routeArr))
            return routes;
        foreach (var r in routeArr.EnumerateArray())
        {
            var prefix = r.TryGetProperty("prefix", out var p) ? p.GetString() : "";
            var nextHop = r.TryGetProperty("nextHop", out var n) ? n.GetString() : "";
            var pref = r.TryGetProperty("preference", out var pr) && pr.TryGetInt32(out var prVal) ? prVal : (int?)null;
            var tag = r.TryGetProperty("tag", out var t) ? t.GetString() : null;
            routes.Add(new StaticRoute(prefix ?? "", nextHop ?? "", pref, tag));
        }
        return routes;
    }

    private static SystemConfig? ParseSystemConfig(JsonElement root)
    {
        if (!root.TryGetProperty("system", out var sys))
            return null;
        var name = sys.TryGetProperty("name", out var n) ? n.GetString() : null;
        var domain = sys.TryGetProperty("domainName", out var d) ? d.GetString() : null;
        return new SystemConfig(name, domain, null, null, null);
    }

    private static DeviceStatus? ParseDeviceStatus(JsonElement root)
    {
        if (!root.TryGetProperty("system", out var sys))
            return null;
        var hostname = sys.TryGetProperty("name", out var n) ? n.GetString() : "unknown";
        var version = sys.TryGetProperty("version", out var v) ? v.GetString() : "unknown";
        return new DeviceStatus(hostname ?? "unknown", version ?? "unknown", "unknown",
            "unknown", 0, 0, 0, Array.Empty<InterfaceStatus>());
    }

    private static DeviceInventory? ParseInventory(JsonElement root)
    {
        if (!root.TryGetProperty("chassis", out var chassis))
            return null;
        var model = chassis.TryGetProperty("model", out var m) ? m.GetString() : "unknown";
        var serial = chassis.TryGetProperty("serialNumber", out var s) ? s.GetString() : "unknown";
        return new DeviceInventory(model ?? "unknown", serial ?? "unknown", "unknown",
            "unknown", "unknown", Array.Empty<ChassisComponent>());
    }

    private static List<AlarmInfo> ParseAlarms(JsonElement root)
    {
        var alarms = new List<AlarmInfo>();
        if (!root.TryGetProperty("alarm", out var alarmArr))
            return alarms;
        foreach (var a in alarmArr.EnumerateArray())
        {
            var severity = a.TryGetProperty("severity", out var sev) ? sev.GetString() : "unknown";
            var desc = a.TryGetProperty("description", out var d) ? d.GetString() : "";
            var source = a.TryGetProperty("source", out var src) ? src.GetString() : null;
            alarms.Add(new AlarmInfo("", severity ?? "unknown", desc ?? "", source, null));
        }
        return alarms;
    }
}
