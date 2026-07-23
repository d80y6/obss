using System.Text.Json;
using Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;
using Obss.Provisioning.Infrastructure.Adapters.Common;
using Obss.Provisioning.Infrastructure.Transports.Restconf;

namespace Obss.Provisioning.Infrastructure.Adapters.Cisco;

public sealed class CiscoRouterAdapter : ICiscoRouterAdapter, IDisposable
{
    private readonly IRestconfTransport _transport;

    public string AdapterName => CiscoAdapterConstants.AdapterName;

    public CiscoRouterAdapter(CiscoAdapterConfig config, IRestconfTransport transport)
    {
        _ = config;
        _transport = transport;
    }

    private static string BuildInterfacePath(string type, string name)
    {
        return $"{CiscoAdapterConstants.InterfacePath}/{type}={name}";
    }

    public async Task<AdapterResult<InterfaceConfig>> ConfigureInterfaceAsync(InterfaceConfig config)
    {
        var (_, ifaceName) = ParseInterfaceName(config.Name);
        var path = BuildInterfacePath(config.Type, ifaceName);
        var payload = CreateInterfacePayload(config);
        var result = await _transport.PutAsync(path, payload);
        return result.Success
            ? AdapterResult<InterfaceConfig>.Success(config)
            : AdapterResult<InterfaceConfig>.Failure(result.ErrorMessage ?? "Unknown error");
    }

    public async Task<AdapterResult<InterfaceConfig>> GetInterfaceAsync(string interfaceName)
    {
        var (type, name) = ParseInterfaceName(interfaceName);
        var path = BuildInterfacePath(type, name);
        var result = await _transport.GetAsync(path);

        if (!result.Success)
            return AdapterResult<InterfaceConfig>.Failure(result.ErrorMessage ?? "Unknown error");

        try
        {
            using var doc = JsonDocument.Parse(result.Data ?? "{}");
            var iface = ParseInterfaceFromResponse(doc.RootElement, type, interfaceName);
            return iface is not null
                ? AdapterResult<InterfaceConfig>.Success(iface)
                : AdapterResult<InterfaceConfig>.Failure("Failed to parse interface");
        }
        catch
        {
            return AdapterResult<InterfaceConfig>.Failure("Failed to parse interface response");
        }
    }

    public async Task<AdapterResult<IReadOnlyList<InterfaceStatus>>> GetInterfaceStatusesAsync()
    {
        var result = await _transport.GetAsync(CiscoAdapterConstants.DeviceStatusPath);
        if (!result.Success)
            return AdapterResult<IReadOnlyList<InterfaceStatus>>.Failure(result.ErrorMessage ?? "Unknown error");

        try
        {
            using var doc = JsonDocument.Parse(result.Data ?? "{}");
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
        var (type, name) = ParseInterfaceName(interfaceName);
        var path = BuildInterfacePath(type, name);
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
                id = config.AsNumber,
                routerId = config.RouterId,
                neighbor = config.Neighbors?.Select(n => new
                {
                    id = n.RemoteAddress,
                    remoteAs = n.RemoteAs,
                    peerGroup = n.PeerGroup
                }).ToArray(),
                network = config.Networks?.Select(n => new
                {
                    id = $"{n.Prefix}/{n.PrefixLength}",
                    backdoor = (string?)null
                }).ToArray()
            }
        };
        var result = await _transport.PutAsync(CiscoAdapterConstants.BgpPath, payload);
        return result.Success
            ? AdapterResult<BgpConfig>.Success(config)
            : AdapterResult<BgpConfig>.Failure(result.ErrorMessage ?? "Unknown error");
    }

    public async Task<AdapterResult<BgpConfig>> GetBgpConfigAsync()
    {
        var result = await _transport.GetAsync(CiscoAdapterConstants.BgpPath);
        if (!result.Success)
            return AdapterResult<BgpConfig>.Failure(result.ErrorMessage ?? "Unknown error");
        try
        {
            using var doc = JsonDocument.Parse(result.Data ?? "{}");
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
                id = config.ProcessId,
                routerId = config.RouterId,
                area = config.Areas?.Select(a => new
                {
                    id = a.AreaId,
                    @interface = a.Interfaces?.ToArray()
                }).ToArray()
            }
        };
        var result = await _transport.PutAsync(CiscoAdapterConstants.OspfPath, payload);
        return result.Success
            ? AdapterResult<OspfConfig>.Success(config)
            : AdapterResult<OspfConfig>.Failure(result.ErrorMessage ?? "Unknown error");
    }

    public async Task<AdapterResult> ConfigureStaticRouteAsync(StaticRoute route)
    {
        var payload = new
        {
            route = new
            {
                via = new[]
                {
                    new
                    {
                        prefix = route.Prefix,
                        nextHop = new[]
                        {
                            new
                            {
                                forwardingAddress = route.NextHop,
                                metric = route.AdministrativeDistance
                            }
                        }
                    }
                }
            }
        };
        var result = await _transport.PostAsync(CiscoAdapterConstants.StaticRoutePath, payload);
        return result.Success
            ? AdapterResult.Ok()
            : AdapterResult.Fail(result.ErrorMessage ?? "Unknown error");
    }

    public async Task<AdapterResult<IReadOnlyList<StaticRoute>>> GetStaticRoutesAsync()
    {
        var result = await _transport.GetAsync(CiscoAdapterConstants.StaticRoutePath);
        if (!result.Success)
            return AdapterResult<IReadOnlyList<StaticRoute>>.Failure(result.ErrorMessage ?? "Unknown error");
        try
        {
            using var doc = JsonDocument.Parse(result.Data ?? "{}");
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
            hostname = config.Hostname,
            ip = new
            {
                domain = new
                {
                    name = config.DomainName,
                    nameServer = config.DnsServers?.Select(d => new { address = d }).ToArray()
                }
            },
            ntp = config.NtpServers?.Select(n => new { server = new { name = n } }).ToArray()
        };
        var result = await _transport.PutAsync(CiscoAdapterConstants.SystemPath, payload);
        return result.Success
            ? AdapterResult<SystemConfig>.Success(config)
            : AdapterResult<SystemConfig>.Failure(result.ErrorMessage ?? "Unknown error");
    }

    public async Task<AdapterResult<SystemConfig>> GetSystemConfigAsync()
    {
        var result = await _transport.GetAsync(CiscoAdapterConstants.SystemPath);
        if (!result.Success)
            return AdapterResult<SystemConfig>.Failure(result.ErrorMessage ?? "Unknown error");
        try
        {
            using var doc = JsonDocument.Parse(result.Data ?? "{}");
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

    public async Task<AdapterResult<AclConfig>> ConfigureAclAsync(AclConfig config)
    {
        var payload = new
        {
            accessListEntries = config.Entries.Select(e => new
            {
                sequence = e.Sequence,
                action = e.Action,
                source = new
                {
                    address = e.Source,
                    wildcard = e.SourceWildcard
                },
                destination = e.Destination is not null ? new { address = e.Destination } : null,
                log = e.Log
            }).ToArray()
        };
        var result = await _transport.PutAsync($"{CiscoAdapterConstants.AclPath}/{config.Name}", payload);
        return result.Success
            ? AdapterResult<AclConfig>.Success(config)
            : AdapterResult<AclConfig>.Failure(result.ErrorMessage ?? "Unknown error");
    }

    public async Task<AdapterResult<DeviceStatus>> GetDeviceStatusAsync()
    {
        var result = await _transport.GetAsync(CiscoAdapterConstants.DeviceStatusPath);
        if (!result.Success)
            return AdapterResult<DeviceStatus>.Failure(result.ErrorMessage ?? "Unknown error");
        try
        {
            using var doc = JsonDocument.Parse(result.Data ?? "{}");
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
        var result = await _transport.GetAsync(CiscoAdapterConstants.InventoryPath);
        if (!result.Success)
            return AdapterResult<DeviceInventory>.Failure(result.ErrorMessage ?? "Unknown error" ?? "Unknown error");
        try
        {
            using var doc = JsonDocument.Parse(result.Data ?? "{}");
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
        var result = await _transport.GetAsync(CiscoAdapterConstants.AlarmsPath);
        if (!result.Success)
            return AdapterResult<IReadOnlyList<AlarmInfo>>.Failure(result.ErrorMessage ?? "Unknown error" ?? "Unknown error");
        try
        {
            using var doc = JsonDocument.Parse(result.Data ?? "{}");
            var alarms = ParseAlarms(doc.RootElement);
            return AdapterResult<IReadOnlyList<AlarmInfo>>.Success(alarms);
        }
        catch
        {
            return AdapterResult<IReadOnlyList<AlarmInfo>>.Failure("Failed to parse alarms");
        }
    }

    public void Dispose()
    {
    }

    private static string? PrefixLengthToSubnetMask(int? prefixLength)
    {
        if (!prefixLength.HasValue)
            return null;
        return string.Join(".", Enumerable.Range(0, 4).Select(i =>
        {
            var bits = Math.Max(0, Math.Min(8, prefixLength.Value - i * 8));
            return (255 << (8 - bits) & 0xFF).ToString();
        }));
    }

    private static (string type, string name) ParseInterfaceName(string interfaceName)
    {
        var typeEnd = interfaceName.IndexOfAny("0123456789".ToCharArray());
        return typeEnd > 0
            ? (interfaceName[..typeEnd], interfaceName[typeEnd..])
            : (interfaceName, "");
    }

    private static object CreateInterfacePayload(InterfaceConfig config)
    {
        return new
        {
            name = config.Name,
            description = config.Description,
            ip = config.IpAddress is not null ? new
            {
                address = new
                {
                    primary = new
                    {
                        address = config.IpAddress,
                        mask = PrefixLengthToSubnetMask(config.PrefixLength)
                    }
                }
            } : null,
            shutdown = config.AdminUp == false,
            mtu = config.Mtu,
            vlan = config.VlanId is not null ? new { id = config.VlanId } : null,
            switchport = config.SwitchportModes?.Any() == true ? new { mode = "access" } : null
        };
    }

    private static InterfaceConfig? ParseInterfaceFromResponse(JsonElement root, string type, string fullName)
    {
        if (!root.TryGetProperty($"Cisco-IOS-XE-native:{type}", out var iface))
            return null;
        var name = iface.TryGetProperty("name", out var n) ? type + n.GetString() : fullName;
        var desc = iface.TryGetProperty("description", out var d) ? d.GetString() : null;
        return new InterfaceConfig(name ?? fullName, type, desc, null, null, null, null, null, null);
    }

    private static List<InterfaceStatus> ParseInterfaceStatuses(JsonElement root)
    {
        var statuses = new List<InterfaceStatus>();
        if (!root.TryGetProperty("Cisco-IOS-XE-native:native", out var native))
            return statuses;
        if (!native.TryGetProperty("interface", out var ifaces))
            return statuses;
        foreach (var ifaceType in ifaces.EnumerateObject())
        {
            foreach (var iface in ifaceType.Value.EnumerateArray())
            {
                var name = iface.TryGetProperty("name", out var n) ? $"{ifaceType.Name}{n.GetString()}" : ifaceType.Name;
                statuses.Add(new InterfaceStatus(name, "up", "up", 1000000));
            }
        }
        return statuses;
    }

    private static BgpConfig? ParseBgpConfig(JsonElement root)
    {
        if (!root.TryGetProperty("Cisco-IOS-XE-bgp:bgp", out var bgp))
            return null;
        var asNumber = bgp.TryGetProperty("id", out var id) ? id.GetInt32() : 0;
        var routerId = bgp.TryGetProperty("routerId", out var rid) ? rid.GetString() : null;
        return new BgpConfig(asNumber, routerId, null, null);
    }

    private static List<StaticRoute> ParseStaticRoutes(JsonElement root)
    {
        var routes = new List<StaticRoute>();
        if (!root.TryGetProperty("Cisco-IOS-XE-native:route", out var route))
            return routes;
        if (!route.TryGetProperty("via", out var via))
            return routes;
        foreach (var v in via.EnumerateArray())
        {
            var prefix = v.TryGetProperty("prefix", out var p) ? p.GetString() : "";
            var nextHop = "";
            if (v.TryGetProperty("nextHop", out var nh) && nh.GetArrayLength() > 0)
                nextHop = nh[0].TryGetProperty("forwardingAddress", out var fa) ? fa.GetString() : "";
            routes.Add(new StaticRoute(prefix ?? "", nextHop ?? "", null, null));
        }
        return routes;
    }

    private static SystemConfig? ParseSystemConfig(JsonElement root)
    {
        if (!root.TryGetProperty("Cisco-IOS-XE-native:native", out var native))
            return null;
        var hostname = native.TryGetProperty("hostname", out var h) ? h.GetString() : null;
        return new SystemConfig(hostname, null, null, null, null);
    }

    private static DeviceStatus? ParseDeviceStatus(JsonElement root)
    {
        if (!root.TryGetProperty("Cisco-IOS-XE-native:native", out var native))
            return null;
        var hostname = native.TryGetProperty("hostname", out var h) ? h.GetString() : "unknown";
        var version = native.TryGetProperty("version", out var v) ? v.GetString() : "unknown";
        var interfaces = ParseInterfaceStatuses(root);
        return new DeviceStatus(hostname ?? "unknown", version ?? "unknown", "Cisco",
            "unknown", 0, 0, interfaces);
    }

    private static DeviceInventory? ParseInventory(JsonElement root)
    {
        if (!root.TryGetProperty("Cisco-IOS-XE-device-hardware:device-hardware", out var hw))
            return null;
        var model = hw.TryGetProperty("model", out var m) ? m.GetString() : "unknown";
        var serial = hw.TryGetProperty("serial-number", out var s) ? s.GetString() : "unknown";
        return new DeviceInventory(model ?? "unknown", serial ?? "unknown", "unknown",
            "unknown", "unknown", Array.Empty<HardwareComponent>());
    }

    private static List<AlarmInfo> ParseAlarms(JsonElement root)
    {
        var alarms = new List<AlarmInfo>();
        if (!root.TryGetProperty("Cisco-IOS-XE-alarms:alarms", out var alarmsEl))
            return alarms;
        if (!alarmsEl.TryGetProperty("alarm", out var alarmArr))
            return alarms;
        foreach (var a in alarmArr.EnumerateArray())
        {
            var severity = a.TryGetProperty("severity", out var sev) ? sev.GetString() : "unknown";
            var desc = a.TryGetProperty("alarm-description", out var d) ? d.GetString() : "";
            var source = a.TryGetProperty("alarm-source", out var src) ? src.GetString() : null;
            alarms.Add(new AlarmInfo("", severity ?? "unknown", desc ?? "", source));
        }
        return alarms;
    }
}
