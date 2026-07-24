using System.Text.Json;
using Obss.Provisioning.Infrastructure.Adapters.Common;
using Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;
using Obss.Provisioning.Infrastructure.Transports.Restconf;

namespace Obss.Provisioning.Infrastructure.Adapters.Juniper;

public sealed class JuniperRouterAdapter : IJuniperRouterAdapter, IDisposable
{
    private readonly IRestconfTransport _transport;

    public string AdapterName => JuniperAdapterConstants.AdapterName;

    public JuniperRouterAdapter(JuniperAdapterConfig config, IRestconfTransport transport)
    {
        _transport = transport;
    }

    public async Task<AdapterResult<InterfaceConfig>> ConfigureInterfaceAsync(InterfaceConfig config)
    {
        var path = $"{JuniperAdapterConstants.InterfacePath}/name={config.Name}";
        object? unitPayload = null;
        if (config.Unit is not null)
        {
            object? inetPayload = null;
            if (config.IpAddress is not null)
            {
                inetPayload = new
                {
                    address = new[]
                    {
                        new
                        {
                            name = $"{config.IpAddress}/{config.PrefixLength ?? 24}"
                        }
                    }
                };
            }
            unitPayload = new[]
            {
                new
                {
                    name = config.Unit ?? "0",
                    vlanId = config.VlanId,
                    family = new { inet = inetPayload }
                }
            };
        }
        var payload = new
        {
            @interface = new
            {
                name = config.Name,
                description = config.Description,
                unit = unitPayload,
                mtu = config.Mtu
            }
        };
        var result = await _transport.PutAsync(path, payload);
        return result.Success
            ? AdapterResult<InterfaceConfig>.Success(config)
            : AdapterResult<InterfaceConfig>.Failure(result.ErrorMessage ?? "Unknown error");
    }

    public async Task<AdapterResult<InterfaceConfig>> GetInterfaceAsync(string interfaceName)
    {
        var path = $"{JuniperAdapterConstants.InterfacePath}/name={interfaceName}";
        var result = await _transport.GetAsync(path);
        if (!result.Success)
            return AdapterResult<InterfaceConfig>.Failure(result.ErrorMessage ?? "Unknown error");

        try
        {
            using var doc = JsonDocument.Parse(result.Data!);
            var config = ParseInterfaceConfig(doc.RootElement, interfaceName);
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
        var result = await _transport.GetAsync(JuniperAdapterConstants.InterfaceStatusPath);
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

    public async Task<AdapterResult> DeleteInterfaceAsync(string interfaceName)
    {
        var path = $"{JuniperAdapterConstants.InterfacePath}/name={interfaceName}";
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
        var result = await _transport.PutAsync(JuniperAdapterConstants.BgpPath, payload);
        return result.Success
            ? AdapterResult<BgpConfig>.Success(config)
            : AdapterResult<BgpConfig>.Failure(result.ErrorMessage ?? "Unknown error");
    }

    public async Task<AdapterResult<BgpConfig>> GetBgpConfigAsync()
    {
        var result = await _transport.GetAsync(JuniperAdapterConstants.BgpPath);
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
                    name = a.AreaId,
                    @interface = a.Interfaces?.Select(i => new { name = i }).ToArray()
                }).ToArray()
            }
        };
        var result = await _transport.PutAsync(JuniperAdapterConstants.OspfPath, payload);
        return result.Success
            ? AdapterResult<OspfConfig>.Success(config)
            : AdapterResult<OspfConfig>.Failure(result.ErrorMessage ?? "Unknown error");
    }

    public async Task<AdapterResult> ConfigureStaticRouteAsync(StaticRoute route)
    {
        var payload = new
        {
            route = new[]
            {
                new
                {
                    name = route.Prefix,
                    nextHop = new[] { new { name = route.NextHop } },
                    preference = route.Preference,
                    tag = route.Tag,
                    qualifiedNextHop = route.QualifiedNextHop is not null
                        ? new[] { new { name = route.QualifiedNextHop } }
                        : null
                }
            }
        };
        var result = await _transport.PostAsync(JuniperAdapterConstants.StaticRoutePath, payload);
        return result.Success
            ? AdapterResult.Ok()
            : AdapterResult.Fail(result.ErrorMessage ?? "Unknown error");
    }

    public async Task<AdapterResult<IReadOnlyList<StaticRoute>>> GetStaticRoutesAsync()
    {
        var result = await _transport.GetAsync(JuniperAdapterConstants.StaticRoutePath);
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
                hostName = config.Hostname,
                domainName = config.DomainName,
                nameServer = config.DnsServers?.Select(d => new { name = d }).ToArray(),
                ntp = config.NtpServers?.Any() == true ? new
                {
                    server = config.NtpServers.Select(n => new { name = n }).ToArray()
                } : null,
                syslog = config.SyslogHost is not null ? new
                {
                    host = new[] { new { name = config.SyslogHost } }
                } : null
            }
        };
        var result = await _transport.PutAsync(JuniperAdapterConstants.SystemPath, payload);
        return result.Success
            ? AdapterResult<SystemConfig>.Success(config)
            : AdapterResult<SystemConfig>.Failure(result.ErrorMessage ?? "Unknown error");
    }

    public async Task<AdapterResult<SystemConfig>> GetSystemConfigAsync()
    {
        var result = await _transport.GetAsync(JuniperAdapterConstants.SystemPath);
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

    public async Task<AdapterResult<FirewallFilterConfig>> ConfigureFirewallFilterAsync(FirewallFilterConfig config)
    {
        var payload = new
        {
            filter = new
            {
                name = config.Name,
                term = config.Terms.Select(t => new
                {
                    name = t.Name,
                    then = new { action = t.Action },
                    from = new
                    {
                        sourceAddress = t.SourceAddress,
                        destinationAddress = t.DestinationAddress,
                        sourcePort = t.SourcePort,
                        destinationPort = t.DestinationPort,
                        protocol = t.Protocol
                    }
                }).ToArray()
            }
        };
        var result = await _transport.PutAsync($"{JuniperAdapterConstants.FirewallFilterPath}/name={config.Name}", payload);
        return result.Success
            ? AdapterResult<FirewallFilterConfig>.Success(config)
            : AdapterResult<FirewallFilterConfig>.Failure(result.ErrorMessage ?? "Unknown error");
    }

    public async Task<AdapterResult<DeviceStatus>> GetDeviceStatusAsync()
    {
        var result = await _transport.GetAsync(JuniperAdapterConstants.DeviceStatusPath);
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
        var result = await _transport.GetAsync(JuniperAdapterConstants.InventoryPath);
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
        var result = await _transport.GetAsync(JuniperAdapterConstants.AlarmsPath);
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
        var result = await _transport.GetAsync(JuniperAdapterConstants.DeviceStatusPath);
        if (!result.Success)
            return AdapterResult<DeviceStatus>.Failure(result.ErrorMessage ?? "Unknown error");
        return await GetDeviceStatusAsync();
    }

    public void Dispose()
    {
    }

    private static InterfaceConfig? ParseInterfaceConfig(JsonElement root, string interfaceName)
    {
        if (!root.TryGetProperty("interface", out var iface))
            return null;
        var name = iface.TryGetProperty("name", out var n) ? n.GetString() : interfaceName;
        var desc = iface.TryGetProperty("description", out var d) ? d.GetString() : null;
        var mtu = iface.TryGetProperty("mtu", out var m) && m.TryGetInt32(out var mtuVal) ? mtuVal : (int?)null;

        string? ipAddress = null;
        int? prefixLength = null;
        if (iface.TryGetProperty("unit", out var unit) && unit.GetArrayLength() > 0)
        {
            var firstUnit = unit[0];
            if (firstUnit.TryGetProperty("family", out var family) &&
                family.TryGetProperty("inet", out var inet) &&
                inet.TryGetProperty("address", out var addr) &&
                addr.GetArrayLength() > 0)
            {
                var addrStr = addr[0].TryGetProperty("name", out var a) ? a.GetString() : null;
                if (addrStr?.Contains('/') == true)
                {
                    var parts = addrStr.Split('/');
                    ipAddress = parts[0];
                    prefixLength = int.TryParse(parts[1], out var pl) ? pl : null;
                }
                else
                {
                    ipAddress = addrStr;
                }
            }
        }

        return new InterfaceConfig(name ?? interfaceName, desc ?? "", null, null, ipAddress, prefixLength, null, mtu);
    }

    private static List<InterfaceStatus> ParseInterfaceStatuses(JsonElement root)
    {
        var statuses = new List<InterfaceStatus>();
        if (!root.TryGetProperty("interface-information", out var info))
            return statuses;
        if (!info.TryGetProperty("physical-interface", out var phys))
            return statuses;
        foreach (var p in phys.EnumerateArray())
        {
            var name = p.TryGetProperty("name", out var n) ? n.GetString() : "unknown";
            var oper = p.TryGetProperty("oper-status", out var os) ? os.GetString() : "unknown";
            var admin = p.TryGetProperty("admin-status", out var ad) ? ad.GetString() : "unknown";
            var speed = p.TryGetProperty("speed", out var s) && long.TryParse(s.GetString(), out var spd) ? spd : 0L;
            statuses.Add(new InterfaceStatus(name ?? "unknown", oper ?? "unknown", admin ?? "unknown", speed));
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
        if (!root.TryGetProperty("route", out var routeArr))
            return routes;
        foreach (var r in routeArr.EnumerateArray())
        {
            var prefix = r.TryGetProperty("name", out var p) ? p.GetString() : "";
            var nextHop = "";
            if (r.TryGetProperty("nextHop", out var nh) && nh.GetArrayLength() > 0)
                nextHop = nh[0].TryGetProperty("name", out var hn) ? hn.GetString() : "";
            var pref = r.TryGetProperty("preference", out var pr) && pr.TryGetInt32(out var prVal) ? prVal : (int?)null;
            var tag = r.TryGetProperty("tag", out var t) ? t.GetString() : null;
            routes.Add(new StaticRoute(prefix ?? "", nextHop ?? "", pref, null, tag));
        }
        return routes;
    }

    private static SystemConfig? ParseSystemConfig(JsonElement root)
    {
        if (!root.TryGetProperty("system", out var sys))
            return null;
        var hostname = sys.TryGetProperty("hostName", out var h) ? h.GetString() : null;
        var domain = sys.TryGetProperty("domainName", out var d) ? d.GetString() : null;
        return new SystemConfig(hostname, domain, null, null, null);
    }

    private static DeviceStatus? ParseDeviceStatus(JsonElement root)
    {
        if (!root.TryGetProperty("system-information", out var info))
            return null;
        var hostname = info.TryGetProperty("hostname", out var h) ? h.GetString() : "unknown";
        var version = info.TryGetProperty("version", out var v) ? v.GetString() : "unknown";
        var model = info.TryGetProperty("model", out var m) ? m.GetString() : "unknown";
        return new DeviceStatus(hostname ?? "unknown", version ?? "unknown", model ?? "unknown",
            "unknown", 0, 0, 0, Array.Empty<InterfaceStatus>());
    }

    private static DeviceInventory? ParseInventory(JsonElement root)
    {
        if (!root.TryGetProperty("chassis-inventory", out var ci))
            return null;
        var model = ci.TryGetProperty("model", out var m) ? m.GetString() : "unknown";
        var serial = ci.TryGetProperty("serial-number", out var s) ? s.GetString() : "unknown";
        return new DeviceInventory(model ?? "unknown", serial ?? "unknown", "unknown",
            "unknown", "unknown", Array.Empty<HardwareComponent>());
    }

    private static List<AlarmInfo> ParseAlarms(JsonElement root)
    {
        var alarms = new List<AlarmInfo>();
        if (!root.TryGetProperty("alarm-information", out var ai))
            return alarms;
        if (!ai.TryGetProperty("alarm", out var alarmArr))
            return alarms;
        foreach (var a in alarmArr.EnumerateArray())
        {
            var severity = a.TryGetProperty("severity", out var sev) ? sev.GetString() : "unknown";
            var desc = a.TryGetProperty("alarm-description", out var d) ? d.GetString() : "";
            var source = a.TryGetProperty("alarm-source", out var src) ? src.GetString() : null;
            alarms.Add(new AlarmInfo("", severity ?? "unknown", desc ?? "", source, null));
        }
        return alarms;
    }
}
