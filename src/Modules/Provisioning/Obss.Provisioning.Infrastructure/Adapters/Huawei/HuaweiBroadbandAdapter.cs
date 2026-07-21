using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Infrastructure.Adapters.Common;
using Obss.Provisioning.Infrastructure.Transports.Abstractions;
using Obss.Provisioning.Infrastructure.Transports.Netconf;
using Obss.Provisioning.Infrastructure.Transports.Rest;
using Obss.Provisioning.Infrastructure.Transports.Snmp;
using Obss.Provisioning.Infrastructure.Transports.Ssh;

namespace Obss.Provisioning.Infrastructure.Adapters.Huawei;

public sealed class HuaweiBroadbandAdapter : HuaweiBroadbandAdapterBase, IHuaweiBroadbandAdapter
{
    private readonly ILogger<HuaweiBroadbandAdapter> _logger;
    private readonly ITransportFactory _transportFactory;

    public HuaweiBroadbandAdapter(
        ILogger<HuaweiBroadbandAdapter> logger,
        HuaweiAdapterConfig config,
        ITransportFactory transportFactory)
        : base(logger, config)
    {
        _logger = logger;
        _transportFactory = transportFactory;
    }

    public override string AdapterName => AdapterConstants.AdapterNames.HuaweiBroadband;

    public string TechnologyType => AdapterConstants.TechnologyTypes.Ftth;

    public async Task<AdapterResult> ActivateFtthAsync(ActivateFtthRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(nameof(ActivateFtthAsync), Guid.NewGuid().ToString("N"), async () =>
        {
            _logger.LogInformation(
                "Activating FTTH: Subscriber={Subscriber}, ONT={Ont}, OLT={Olt}, PON={Pon}",
                request.SubscriberId, request.OntSerial, request.OltHostname, request.PonPort);

            if (Config.TryGetNetconfConfig() is { } netconfConfig)
            {
                var transport = _transportFactory.CreateTransport(netconfConfig);
                var netconf = (INetconfTransport)transport;

                var configXml = BuildFtthConfigXml(request);
                var result = await netconf.EditConfigAsync(configXml, cancellationToken: cancellationToken);

                if (!result.Success)
                    return AdapterResult.Fail($"NETCONF FTTH activation failed: {result.ErrorMessage}", adapterName: AdapterName);

                var verifyResult = await netconf.GetConfigAsync("running", cancellationToken);
                if (!verifyResult.Success)
                    return AdapterResult.VendorPending("FTTH activation submitted, awaiting confirmation", AdapterName);

                var ontId = ExtractOntId(verifyResult.Data);
                return AdapterResult.Ok(
                    JsonSerializer.Serialize(new ActivateFtthResponse(ontId, GenerateCircuitId(), "active", DateTime.UtcNow)),
                    adapterName: AdapterName);
            }

            if (Config.TryGetSshConfig() is { } sshConfig)
            {
                var transport = _transportFactory.CreateTransport(sshConfig);
                var ssh = (ISshTransport)transport;

                var commands = BuildFtthCliCommands(request);
                foreach (var command in commands)
                {
                    var result = await ssh.ExecuteCommandAsync(command, cancellationToken);
                    if (!result.Success)
                        return AdapterResult.Fail($"CLI command failed at '{command}': {result.ErrorMessage}", adapterName: AdapterName);
                }

                return AdapterResult.Ok(
                    JsonSerializer.Serialize(new ActivateFtthResponse(GenerateOntId(), GenerateCircuitId(), "active", DateTime.UtcNow)),
                    adapterName: AdapterName);
            }

            if (Config.TryGetRestConfig() is { } restConfig)
            {
                var transport = _transportFactory.CreateTransport(restConfig);
                var rest = (IRestTransport)transport;

                var result = await rest.PostAsync("/api/olt/ftth-activate", request, cancellationToken);
                if (!result.Success)
                    return AdapterResult.Fail($"REST FTTH activation failed: {result.ErrorMessage}", adapterName: AdapterName);

                return AdapterResult.Ok(result.Data, adapterName: AdapterName);
            }

            return AdapterResult.Blocked(
                "No network transport configured for Huawei OLT. Configure NETCONF, SSH, or REST transport, or enable simulator mode.",
                Guid.NewGuid().ToString("N"),
                AdapterName);
        }, cancellationToken);
    }

    public async Task<AdapterResult> ActivateAdslAsync(ActivateAdslRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(nameof(ActivateAdslAsync), Guid.NewGuid().ToString("N"), async () =>
        {
            _logger.LogInformation(
                "Activating ADSL: Subscriber={Subscriber}, DSLAM={Dslam}, Port={Port}",
                request.SubscriberId, request.DslamHostname, request.DslamPort);

            if (Config.TryGetSshConfig() is { } sshConfig)
            {
                var transport = _transportFactory.CreateTransport(sshConfig);
                var ssh = (ISshTransport)transport;

                var commands = BuildAdslCliCommands(request);
                foreach (var command in commands)
                {
                    var result = await ssh.ExecuteCommandAsync(command, cancellationToken);
                    if (!result.Success)
                        return AdapterResult.Fail($"ADSL CLI command failed: {result.ErrorMessage}", adapterName: AdapterName);
                }

                return AdapterResult.Ok(
                    JsonSerializer.Serialize(new ActivateAdslResponse(request.DslamPort, GenerateCircuitId(), "active", DateTime.UtcNow)),
                    adapterName: AdapterName);
            }

            return AdapterResult.Blocked(
                "No SSH transport configured for DSLAM. Configure SSH transport or enable simulator mode.",
                Guid.NewGuid().ToString("N"), AdapterName);
        }, cancellationToken);
    }

    public async Task<AdapterResult> Activate4GAsync(Activate4GRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(nameof(Activate4GAsync), Guid.NewGuid().ToString("N"), async () =>
        {
            _logger.LogInformation(
                "Activating 4G: Subscriber={Subscriber}, IMSI={Imsi}, APN={Apn}",
                request.SubscriberId, request.Imsi, request.ApnName);

            if (Config.TryGetRestConfig() is { } restConfig)
            {
                var transport = _transportFactory.CreateTransport(restConfig);
                var rest = (IRestTransport)transport;

                var result = await rest.PostAsync("/api/pcrf/subscriber", request, cancellationToken);
                if (!result.Success)
                    return AdapterResult.Fail($"4G activation via PCRF failed: {result.ErrorMessage}", adapterName: AdapterName);

                return AdapterResult.Ok(
                    JsonSerializer.Serialize(new Activate4GResponse(
                        Guid.NewGuid().ToString("N"), "1", "1", "active", DateTime.UtcNow)),
                    adapterName: AdapterName);
            }

            return AdapterResult.Blocked(
                "No REST transport configured for PCRF/AAA. Configure REST transport or enable simulator mode.",
                Guid.NewGuid().ToString("N"), AdapterName);
        }, cancellationToken);
    }

    public async Task<AdapterResult> ActivateWiFiAsync(ActivateWiFiRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(nameof(ActivateWiFiAsync), Guid.NewGuid().ToString("N"), async () =>
        {
            _logger.LogInformation("Activating WiFi: SSID={Ssid}", request.Ssid);

            if (Config.TryGetNetconfConfig() is { } netconfConfig)
            {
                var transport = _transportFactory.CreateTransport(netconfConfig);
                var netconf = (INetconfTransport)transport;

                var wifiConfig = BuildWifiConfigXml(request);
                var result = await netconf.EditConfigAsync(wifiConfig, cancellationToken: cancellationToken);

                if (!result.Success)
                    return AdapterResult.Fail($"WiFi configuration failed: {result.ErrorMessage}", adapterName: AdapterName);

                return AdapterResult.Ok(
                    JsonSerializer.Serialize(new ActivateWiFiResponse(Guid.NewGuid().ToString("N"), "active", DateTime.UtcNow)),
                    adapterName: AdapterName);
            }

            return AdapterResult.Blocked(
                "No NETCONF transport configured for WiFi configuration. Configure NETCONF or enable simulator mode.",
                Guid.NewGuid().ToString("N"), AdapterName);
        }, cancellationToken);
    }

    public async Task<AdapterResult> SuspendServiceAsync(SuspendRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(nameof(SuspendServiceAsync), Guid.NewGuid().ToString("N"), async () =>
        {
            _logger.LogInformation("Suspending service: Subscriber={Subscriber}", request.SubscriberId);

            if (Config.TryGetNetconfConfig() is { } netconfConfig)
            {
                var transport = _transportFactory.CreateTransport(netconfConfig);
                var netconf = (INetconfTransport)transport;

                var suspendXml = BuildSuspendConfigXml(request);
                var result = await netconf.EditConfigAsync(suspendXml, cancellationToken: cancellationToken);

                if (!result.Success)
                    return AdapterResult.Fail($"Suspend via NETCONF failed: {result.ErrorMessage}", adapterName: AdapterName);

                return AdapterResult.Ok(
                    JsonSerializer.Serialize(new SuspendResponse(DateTime.UtcNow, DateTime.UtcNow.AddDays(30), "suspended")),
                    adapterName: AdapterName);
            }

            return AdapterResult.Blocked(
                "No NETCONF transport configured for service suspension.",
                Guid.NewGuid().ToString("N"), AdapterName);
        }, cancellationToken);
    }

    public async Task<AdapterResult> ResumeServiceAsync(ResumeRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(nameof(ResumeServiceAsync), Guid.NewGuid().ToString("N"), async () =>
        {
            _logger.LogInformation("Resuming service: Subscriber={Subscriber}", request.SubscriberId);

            if (Config.TryGetNetconfConfig() is { } netconfConfig)
            {
                var transport = _transportFactory.CreateTransport(netconfConfig);
                var netconf = (INetconfTransport)transport;

                var resumeXml = BuildResumeConfigXml(request);
                var result = await netconf.EditConfigAsync(resumeXml, cancellationToken: cancellationToken);

                if (!result.Success)
                    return AdapterResult.Fail($"Resume via NETCONF failed: {result.ErrorMessage}", adapterName: AdapterName);

                return AdapterResult.Ok(
                    JsonSerializer.Serialize(new ResumeResponse(DateTime.UtcNow, "active")),
                    adapterName: AdapterName);
            }

            return AdapterResult.Blocked(
                "No NETCONF transport configured for service resume.",
                Guid.NewGuid().ToString("N"), AdapterName);
        }, cancellationToken);
    }

    public async Task<AdapterResult> ChangeServiceAsync(ChangeServiceRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(nameof(ChangeServiceAsync), Guid.NewGuid().ToString("N"), async () =>
        {
            _logger.LogInformation("Changing service: Subscriber={Subscriber}, Speed={Speed}",
                request.SubscriberId, request.NewBandwidthMbps);

            if (Config.TryGetNetconfConfig() is { } netconfConfig)
            {
                var transport = _transportFactory.CreateTransport(netconfConfig);
                var netconf = (INetconfTransport)transport;

                var changeXml = BuildChangeServiceConfigXml(request);
                var result = await netconf.EditConfigAsync(changeXml, cancellationToken: cancellationToken);

                if (!result.Success)
                    return AdapterResult.Fail($"Service change via NETCONF failed: {result.ErrorMessage}", adapterName: AdapterName);

                return AdapterResult.Ok(
                    JsonSerializer.Serialize(new ChangeServiceResponse(
                        DateTime.UtcNow, "old", request.NewBandwidthMbps?.ToString() ?? "", "changed")),
                    adapterName: AdapterName);
            }

            return AdapterResult.Blocked(
                "No NETCONF transport configured for service changes.",
                Guid.NewGuid().ToString("N"), AdapterName);
        }, cancellationToken);
    }

    public async Task<AdapterResult> TerminateServiceAsync(TerminateRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(nameof(TerminateServiceAsync), Guid.NewGuid().ToString("N"), async () =>
        {
            _logger.LogInformation("Terminating service: Subscriber={Subscriber}", request.SubscriberId);

            if (request.Immediate)
            {
                return AdapterResult.Blocked(
                    "Immediate termination requires operator approval. Schedule termination instead.",
                    Guid.NewGuid().ToString("N"), AdapterName);
            }

            if (Config.TryGetNetconfConfig() is { } netconfConfig)
            {
                var transport = _transportFactory.CreateTransport(netconfConfig);
                var netconf = (INetconfTransport)transport;

                var terminateXml = BuildTerminateConfigXml(request);
                var result = await netconf.EditConfigAsync(terminateXml, cancellationToken: cancellationToken);

                if (!result.Success)
                    return AdapterResult.Fail($"Termination via NETCONF failed: {result.ErrorMessage}", adapterName: AdapterName);

                return AdapterResult.Ok(
                    JsonSerializer.Serialize(new TerminateResponse(DateTime.UtcNow.AddDays(7), true, "scheduled-for-termination")),
                    adapterName: AdapterName);
            }

            return AdapterResult.Blocked(
                "No NETCONF transport configured for service termination.",
                Guid.NewGuid().ToString("N"), AdapterName);
        }, cancellationToken);
    }

    public async Task<AdapterResult> GetDeviceStatusAsync(DeviceStatusRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(nameof(GetDeviceStatusAsync), Guid.NewGuid().ToString("N"), async () =>
        {
            _logger.LogInformation("Getting device status: {Device}", request.DeviceHostname);

            if (Config.TryGetSnmpConfig() is { } snmpConfig)
            {
                var transport = _transportFactory.CreateTransport(snmpConfig);
                var snmp = (ISnmpTransport)transport;

                var uptimeResult = await snmp.GetAsync("1.3.6.1.2.1.1.3.0", cancellationToken);
                var cpuResult = await snmp.GetAsync("1.3.6.1.4.1.2011.6.1.2.1.1.1.1.5", cancellationToken);
                var memResult = await snmp.GetAsync("1.3.6.1.4.1.2011.6.1.2.1.1.1.1.6", cancellationToken);
                var tempResult = await snmp.GetAsync("1.3.6.1.4.1.2011.6.1.2.1.1.1.1.7", cancellationToken);
                var portsResult = await snmp.WalkAsync("1.3.6.1.2.1.2.2.1.8", cancellationToken);

                var status = "online";
                if (!uptimeResult.Success && !cpuResult.Success)
                    status = "offline";

                var activePorts = portsResult.Success
                    ? portsResult.Data?.Split('\n').Count(l => l.Contains("1")) ?? 0
                    : 0;

                return AdapterResult.Ok(
                    JsonSerializer.Serialize(new DeviceStatusResponse(
                        status,
                        TimeSpan.FromSeconds(double.TryParse(uptimeResult.Data, out var ticks) ? ticks / 100 : 0),
                        double.TryParse(cpuResult.Data, out var cpu) ? cpu : 0,
                        double.TryParse(memResult.Data, out var mem) ? mem : 0,
                        double.TryParse(tempResult.Data, out var temp) ? temp : 0,
                        activePorts,
                        48)),
                    adapterName: AdapterName);
            }

            return AdapterResult.Blocked(
                "No SNMP transport configured for device monitoring.",
                Guid.NewGuid().ToString("N"), AdapterName);
        }, cancellationToken);
    }

    public async Task<AdapterResult> GetAlarmsAsync(AlarmQueryRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(nameof(GetAlarmsAsync), Guid.NewGuid().ToString("N"), async () =>
        {
            _logger.LogInformation("Getting alarms: Device={Device}", request.DeviceHostname);

            if (Config.TryGetSnmpConfig() is { } snmpConfig)
            {
                var transport = _transportFactory.CreateTransport(snmpConfig);
                var snmp = (ISnmpTransport)transport;

                var alarmWalk = await snmp.WalkAsync("1.3.6.1.2.1.43.18.2.1", cancellationToken);

                if (!alarmWalk.Success)
                    return AdapterResult.Fail($"Failed to collect alarms via SNMP: {alarmWalk.ErrorMessage}", adapterName: AdapterName);

                return AdapterResult.Ok(alarmWalk.Data, adapterName: AdapterName);
            }

            return AdapterResult.Blocked(
                "No SNMP transport configured for alarm collection.",
                Guid.NewGuid().ToString("N"), AdapterName);
        }, cancellationToken);
    }

    public async Task<AdapterResult> CollectPerformanceMetricsAsync(MetricsRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(nameof(CollectPerformanceMetricsAsync), Guid.NewGuid().ToString("N"), async () =>
        {
            _logger.LogInformation("Collecting metrics: Device={Device}, Type={Type}",
                request.DeviceHostname, request.MetricType);

            if (Config.TryGetSnmpConfig() is { } snmpConfig)
            {
                var transport = _transportFactory.CreateTransport(snmpConfig);
                var snmp = (ISnmpTransport)transport;

                var metrics = new Dictionary<string, double>();
                var oids = request.MetricType switch
                {
                    "interface" => new[] { "1.3.6.1.2.1.2.2.1.10", "1.3.6.1.2.1.2.2.1.16", "1.3.6.1.2.1.2.2.1.13", "1.3.6.1.2.1.2.2.1.14" },
                    "cpu" => new[] { "1.3.6.1.4.1.2011.6.1.2.1.1.1.1.5" },
                    "memory" => new[] { "1.3.6.1.4.1.2011.6.1.2.1.1.1.1.6" },
                    "temperature" => new[] { "1.3.6.1.4.1.2011.6.1.2.1.1.1.1.7" },
                    _ => Array.Empty<string>()
                };

                foreach (var oid in oids)
                {
                    var result = await snmp.WalkAsync(oid, cancellationToken);
                    if (result.Success && result.Data != null)
                    {
                        var lines = result.Data.Split('\n');
                        foreach (var line in lines)
                        {
                            var parts = line.Split('=');
                            if (parts.Length == 2 && double.TryParse(parts[1].Trim(), out var value))
                                metrics[parts[0].Trim()] = value;
                        }
                    }
                }

                return AdapterResult.Ok(
                    JsonSerializer.Serialize(new MetricsResponse(
                        request.DeviceHostname, request.MetricType, request.Interval, metrics, DateTime.UtcNow)),
                    adapterName: AdapterName);
            }

            return AdapterResult.Blocked(
                "No SNMP transport configured for performance metrics.",
                Guid.NewGuid().ToString("N"), AdapterName);
        }, cancellationToken);
    }

    public async Task<AdapterResult> ReconcileInventoryAsync(ReconcileRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(nameof(ReconcileInventoryAsync), Guid.NewGuid().ToString("N"), async () =>
        {
            _logger.LogInformation("Reconciling inventory: Device={Device}, Type={Type}",
                request.DeviceHostname, request.ResourceType);

            if (Config.TryGetSnmpConfig() is { } snmpConfig)
            {
                var transport = _transportFactory.CreateTransport(snmpConfig);
                var snmp = (ISnmpTransport)transport;

                var inventoryData = request.ResourceType switch
                {
                    "ont" => await snmp.WalkAsync("1.3.6.1.4.1.2011.6.1.2.1.1.2", cancellationToken),
                    "port" => await snmp.WalkAsync("1.3.6.1.2.1.2.2.1", cancellationToken),
                    "vlan" => await snmp.WalkAsync("1.3.6.1.4.1.2011.6.1.2.1.1.3", cancellationToken),
                    _ => TransportResult.Fail($"Unknown resource type: {request.ResourceType}")
                };

                if (!inventoryData.Success)
                    return AdapterResult.Fail(inventoryData.ErrorMessage ?? "Inventory reconciliation failed", adapterName: AdapterName);

                var resources = ParseInventoryData(inventoryData.Data);
                return AdapterResult.Ok(
                    JsonSerializer.Serialize(new ReconcileResponse(
                        request.DeviceHostname, request.ResourceType, resources.Count, resources, DateTime.UtcNow)),
                    adapterName: AdapterName);
            }

            return AdapterResult.Blocked(
                "No SNMP transport configured for inventory reconciliation.",
                Guid.NewGuid().ToString("N"), AdapterName);
        }, cancellationToken);
    }

    private static string BuildFtthConfigXml(ActivateFtthRequest request)
    {
        return $"""
            <ifm:interfaces xmlns:ifm="urn:huawei:yang:huawei-ifm">
                <ifm:interface>
                    <ifm:name>0/{request.PonPort.Replace("/", "/")}</ifm:name>
                    <ifm:ont-id>{GenerateOntId()}</ifm:ont-id>
                    <ifm:ont-sn>{request.OntSerial}</ifm:ont-sn>
                    <ifm:vlan-id>{request.VlanId}</ifm:vlan-id>
                    <ifm:service-vlan>{request.ServiceVlan}</ifm:service-vlan>
                    <ifm:bandwidth>{request.BandwidthMbps}</ifm:bandwidth>
                </ifm:interface>
            </ifm:interfaces>
            """;
    }

    private static string[] BuildFtthCliCommands(ActivateFtthRequest request)
    {
        return
        [
            $"configure terminal",
            $"interface gpon 0/{request.PonPort.Replace("/", "/")}",
            $"ont add {request.OntSerial} {GenerateOntId()}",
            $"ont port native-vlan {GenerateOntId()} vlan {request.VlanId}",
            $"ont port service-port {GenerateOntId()} vlan {request.ServiceVlan}",
            $"ont port bandwidth {GenerateOntId()} upstream {request.BandwidthMbps}",
            $"ont port bandwidth {GenerateOntId()} downstream {request.BandwidthMbps}",
            $"commit",
            $"end"
        ];
    }

    private static string[] BuildAdslCliCommands(ActivateAdslRequest request)
    {
        return
        [
            $"configure terminal",
            $"interface adsl {request.DslamPort}",
            $"line-profile {request.LineProfile}",
            $"pvc {request.Vpi}/{request.Vci}",
            $"commit",
            $"end"
        ];
    }

    private static string BuildWifiConfigXml(ActivateWiFiRequest request)
    {
        return $"""
            <wlan:ap-configs xmlns:wlan="urn:huawei:yang:huawei-wlan">
                <wlan:ap-config>
                    <wlan:ssid>{request.Ssid}</wlan:ssid>
                    <wlan:security>
                        <wlan:mode>{request.Encryption}</wlan:mode>
                        <wlan:passphrase>{request.Passphrase}</wlan:passphrase>
                    </wlan:security>
                    <wlan:radio-band>{request.RadioBand}</wlan:radio-band>
                    <wlan:client-limit>{request.ClientLimit}</wlan:client-limit>
                </wlan:ap-config>
            </wlan:ap-configs>
            """;
    }

    private static string BuildSuspendConfigXml(SuspendRequest request)
    {
        return $"""
            <ifm:interfaces xmlns:ifm="urn:huawei:yang:huawei-ifm">
                <ifm:interface>
                    <ifm:name>{request.SubscriberId}</ifm:name>
                    <ifm:admin-status>down</ifm:admin-status>
                    <ifm:suspend-reason>{request.Reason}</ifm:suspend-reason>
                </ifm:interface>
            </ifm:interfaces>
            """;
    }

    private static string BuildResumeConfigXml(ResumeRequest request)
    {
        return $"""
            <ifm:interfaces xmlns:ifm="urn:huawei:yang:huawei-ifm">
                <ifm:interface>
                    <ifm:name>{request.SubscriberId}</ifm:name>
                    <ifm:admin-status>up</ifm:admin-status>
                </ifm:interface>
            </ifm:interfaces>
            """;
    }

    private static string BuildChangeServiceConfigXml(ChangeServiceRequest request)
    {
        return $"""
            <ifm:interfaces xmlns:ifm="urn:huawei:yang:huawei-ifm">
                <ifm:interface>
                    <ifm:name>{request.SubscriberId}</ifm:name>
                    {request.NewBandwidthMbps.WhenHasValue(b => $"<ifm:bandwidth>{b}</ifm:bandwidth>")}
                    {request.NewVlanId.WhenHasValue(v => $"<ifm:vlan-id>{v}</ifm:vlan-id>")}
                </ifm:interface>
            </ifm:interfaces>
            """;
    }

    private static string BuildTerminateConfigXml(TerminateRequest request)
    {
        return $"""
            <ifm:interfaces xmlns:ifm="urn:huawei:yang:huawei-ifm">
                <ifm:interface operation="delete">
                    <ifm:name>{request.SubscriberId}</ifm:name>
                </ifm:interface>
            </ifm:interfaces>
            """;
    }

    private static string GenerateOntId() => $"ONT-{Random.Shared.Next(100000, 999999)}";

    private static string GenerateCircuitId() => Guid.NewGuid().ToString("N");

    private static string ExtractOntId(string? configData)
    {
        return configData?.Contains("ont-id") == true ? "provisioned" : "unknown";
    }

    private static IReadOnlyCollection<Dictionary<string, string>> ParseInventoryData(string? data)
    {
        if (string.IsNullOrEmpty(data))
            return Array.Empty<Dictionary<string, string>>();

        return data.Split('\n')
            .Select((line, i) => new Dictionary<string, string>
            {
                ["index"] = i.ToString(),
                ["data"] = line
            })
            .ToList();
    }
}

internal static class HuaweiExtensions
{
    public static TResult? Let<T, TResult>(this T? value, Func<T, TResult> func) where T : class
        => value is not null ? func(value) : default;

    public static string WhenHasValue(this int? value, Func<int, string> func)
        => value.HasValue ? func(value.Value) : "";
}
