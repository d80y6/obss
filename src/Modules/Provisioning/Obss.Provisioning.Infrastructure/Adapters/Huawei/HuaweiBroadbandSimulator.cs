using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Infrastructure.Adapters.Common;

namespace Obss.Provisioning.Infrastructure.Adapters.Huawei;

public sealed partial class HuaweiBroadbandSimulator : IHuaweiBroadbandAdapter
{
    private readonly ILogger<HuaweiBroadbandSimulator> _logger;
    private static readonly Random _random = new();
    private static readonly HashSet<string> _validOntSerials = [];
    private static readonly HashSet<string> _validPonPorts = [];

    static HuaweiBroadbandSimulator()
    {
        for (var i = 0; i < 100; i++)
        {
            _validOntSerials.Add($"HWTC{i:X8}");
        }

        for (var i = 0; i < 8; i++)
        {
            for (var j = 0; j < 8; j++)
            {
                _validPonPorts.Add($"{i}/{j}");
            }
        }
    }

    public HuaweiBroadbandSimulator(ILogger<HuaweiBroadbandSimulator> logger, HuaweiAdapterConfig config)
    {
        _logger = logger;
    }

    public string AdapterName => AdapterConstants.AdapterNames.HuaweiBroadband;

    public string TechnologyType => AdapterConstants.TechnologyTypes.Ftth;

    public async Task<AdapterResult> ActivateFtthAsync(ActivateFtthRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[SIM] ActivateFtth: Subscriber={Subscriber}, ONT={Ont}, OLT={Olt}, PON={Pon}",
            request.SubscriberId, request.OntSerial, request.OltHostname, request.PonPort);

        if (string.IsNullOrWhiteSpace(request.SubscriberId))
            return AdapterResult.Fail("SubscriberId is required", adapterName: AdapterName);

        if (!OntSerialRegex().IsMatch(request.OntSerial))
            return AdapterResult.Fail($"Invalid ONT serial format: {request.OntSerial}. Expected format like HWTCXXXXXXXX", adapterName: AdapterName);

        if (!_validPonPorts.Contains(request.PonPort))
            return AdapterResult.Fail($"Invalid PON port: {request.PonPort}. Expected format like 0/1", adapterName: AdapterName);

        if (request.VlanId < 1 || request.VlanId > 4094)
            return AdapterResult.Fail($"Invalid VLAN ID: {request.VlanId}. Must be between 1 and 4094", adapterName: AdapterName);

        if (request.BandwidthMbps <= 0 || request.BandwidthMbps > 10000)
            return AdapterResult.Fail($"Invalid bandwidth: {request.BandwidthMbps} Mbps. Must be between 1 and 10000", adapterName: AdapterName);

        await Task.Delay(_random.Next(100, 300), cancellationToken);

        var response = new ActivateFtthResponse(
            OntId: GenerateOntId(),
            CircuitId: Guid.NewGuid().ToString("N"),
            Status: "active",
            ActivationTimestamp: DateTime.UtcNow);

        _logger.LogInformation("[SIM] ActivateFtth success: OntId={OntId}, CircuitId={CircuitId}", response.OntId, response.CircuitId);

        return AdapterResult.Simulated(
            JsonSerializer.Serialize(response),
            TimeSpan.FromMilliseconds(250),
            AdapterName);
    }

    public async Task<AdapterResult> ActivateAdslAsync(ActivateAdslRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[SIM] ActivateAdsl: Subscriber={Subscriber}, DSLAM={Dslam}, Port={Port}",
            request.SubscriberId, request.DslamHostname, request.DslamPort);

        if (string.IsNullOrWhiteSpace(request.SubscriberId))
            return AdapterResult.Fail("SubscriberId is required", adapterName: AdapterName);

        if (string.IsNullOrWhiteSpace(request.DslamPort))
            return AdapterResult.Fail("DSLAM port is required", adapterName: AdapterName);

        if (request.Vpi < 0 || request.Vpi > 255)
            return AdapterResult.Fail($"Invalid VPI: {request.Vpi}. Must be between 0 and 255", adapterName: AdapterName);

        if (request.Vci < 32 || request.Vci > 65535)
            return AdapterResult.Fail($"Invalid VCI: {request.Vci}. Must be between 32 and 65535", adapterName: AdapterName);

        await Task.Delay(_random.Next(100, 400), cancellationToken);

        var response = new ActivateAdslResponse(
            DslamPortId: Guid.NewGuid().ToString("N"),
            CircuitId: Guid.NewGuid().ToString("N"),
            Status: "active",
            ActivationTimestamp: DateTime.UtcNow);

        return AdapterResult.Simulated(
            JsonSerializer.Serialize(response),
            TimeSpan.FromMilliseconds(300),
            AdapterName);
    }

    public async Task<AdapterResult> Activate4GAsync(Activate4GRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[SIM] Activate4G: Subscriber={Subscriber}, IMSI={Imsi}, MSISDN={Msisdn}, APN={Apn}",
            request.SubscriberId, request.Imsi, request.Msisdn, request.ApnName);

        if (string.IsNullOrWhiteSpace(request.SubscriberId))
            return AdapterResult.Fail("SubscriberId is required", adapterName: AdapterName);

        if (!ImsiRegex().IsMatch(request.Imsi))
            return AdapterResult.Fail($"Invalid IMSI format: {request.Imsi}. Must be 15 digits", adapterName: AdapterName);

        if (!MsisdnRegex().IsMatch(request.Msisdn))
            return AdapterResult.Fail($"Invalid MSISDN format: {request.Msisdn}. Must be 10-15 digits", adapterName: AdapterName);

        if (string.IsNullOrWhiteSpace(request.ApnName))
            return AdapterResult.Fail("APN name is required", adapterName: AdapterName);

        if (string.IsNullOrWhiteSpace(request.QosProfile))
            return AdapterResult.Fail("QoS profile is required", adapterName: AdapterName);

        await Task.Delay(_random.Next(150, 500), cancellationToken);

        var response = new Activate4GResponse(
            NetworkSubscriberId: Guid.NewGuid().ToString("N"),
            PdnId: _random.Next(1, 16).ToString(),
            BearerId: _random.Next(1, 11).ToString(),
            Status: "active",
            ActivationTimestamp: DateTime.UtcNow);

        return AdapterResult.Simulated(
            JsonSerializer.Serialize(response),
            TimeSpan.FromMilliseconds(350),
            AdapterName);
    }

    public async Task<AdapterResult> ActivateWiFiAsync(ActivateWiFiRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[SIM] ActivateWiFi: Subscriber={Subscriber}, SSID={Ssid}",
            request.SubscriberId, request.Ssid);

        if (string.IsNullOrWhiteSpace(request.SubscriberId))
            return AdapterResult.Fail("SubscriberId is required", adapterName: AdapterName);

        if (string.IsNullOrWhiteSpace(request.Ssid) || request.Ssid.Length < 1 || request.Ssid.Length > 32)
            return AdapterResult.Fail($"Invalid SSID: must be 1-32 characters", adapterName: AdapterName);

        if (string.IsNullOrWhiteSpace(request.Passphrase) || request.Passphrase.Length < 8)
            return AdapterResult.Fail($"Invalid passphrase: must be at least 8 characters", adapterName: AdapterName);

        var validEncryptions = new[] { "WPA2", "WPA3", "WPA2/WPA3" };
        if (!validEncryptions.Contains(request.Encryption))
            return AdapterResult.Fail($"Invalid encryption: {request.Encryption}. Must be one of: {string.Join(", ", validEncryptions)}", adapterName: AdapterName);

        var validBands = new[] { "2.4GHz", "5GHz", "dual" };
        if (!validBands.Contains(request.RadioBand))
            return AdapterResult.Fail($"Invalid radio band: {request.RadioBand}. Must be one of: {string.Join(", ", validBands)}", adapterName: AdapterName);

        await Task.Delay(_random.Next(100, 300), cancellationToken);

        var response = new ActivateWiFiResponse(
            WifiProfileId: Guid.NewGuid().ToString("N"),
            Status: "active",
            ActivationTimestamp: DateTime.UtcNow);

        return AdapterResult.Simulated(
            JsonSerializer.Serialize(response),
            TimeSpan.FromMilliseconds(250),
            AdapterName);
    }

    public async Task<AdapterResult> SuspendServiceAsync(SuspendRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[SIM] Suspend: Subscriber={Subscriber}, Service={Service}, Reason={Reason}",
            request.SubscriberId, request.ServiceType, request.Reason);

        if (string.IsNullOrWhiteSpace(request.SubscriberId))
            return AdapterResult.Fail("SubscriberId is required", adapterName: AdapterName);

        if (string.IsNullOrWhiteSpace(request.Reason))
            return AdapterResult.Fail("Suspension reason is required", adapterName: AdapterName);

        await Task.Delay(_random.Next(50, 150), cancellationToken);

        var response = new SuspendResponse(
            SuspensionDate: DateTime.UtcNow,
            ExpectedResumeDate: DateTime.UtcNow.AddDays(30),
            Status: "suspended");

        return AdapterResult.Simulated(
            JsonSerializer.Serialize(response),
            TimeSpan.FromMilliseconds(100),
            AdapterName);
    }

    public async Task<AdapterResult> ResumeServiceAsync(ResumeRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[SIM] Resume: Subscriber={Subscriber}, Service={Service}",
            request.SubscriberId, request.ServiceType);

        if (string.IsNullOrWhiteSpace(request.SubscriberId))
            return AdapterResult.Fail("SubscriberId is required", adapterName: AdapterName);

        await Task.Delay(_random.Next(50, 150), cancellationToken);

        var response = new ResumeResponse(
            ResumeDate: DateTime.UtcNow,
            Status: "active");

        return AdapterResult.Simulated(
            JsonSerializer.Serialize(response),
            TimeSpan.FromMilliseconds(100),
            AdapterName);
    }

    public async Task<AdapterResult> ChangeServiceAsync(ChangeServiceRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[SIM] ChangeService: Subscriber={Subscriber}, Service={Service}, Speed={Speed}",
            request.SubscriberId, request.ServiceType, request.NewBandwidthMbps);

        if (string.IsNullOrWhiteSpace(request.SubscriberId))
            return AdapterResult.Fail("SubscriberId is required", adapterName: AdapterName);

        if (request.NewBandwidthMbps.HasValue && (request.NewBandwidthMbps.Value <= 0 || request.NewBandwidthMbps.Value > 10000))
            return AdapterResult.Fail($"Invalid bandwidth: {request.NewBandwidthMbps} Mbps", adapterName: AdapterName);

        await Task.Delay(_random.Next(100, 300), cancellationToken);

        var oldSpeed = request.NewBandwidthMbps.HasValue
            ? $"{_random.Next(10, request.NewBandwidthMbps.Value)}"
            : "100";

        var response = new ChangeServiceResponse(
            ChangeDate: DateTime.UtcNow,
            OldSpeed: oldSpeed,
            NewSpeed: request.NewBandwidthMbps?.ToString() ?? oldSpeed,
            Status: "changed");

        return AdapterResult.Simulated(
            JsonSerializer.Serialize(response),
            TimeSpan.FromMilliseconds(200),
            AdapterName);
    }

    public async Task<AdapterResult> TerminateServiceAsync(TerminateRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[SIM] Terminate: Subscriber={Subscriber}, Service={Service}, Immediate={Immediate}",
            request.SubscriberId, request.ServiceType, request.Immediate);

        if (string.IsNullOrWhiteSpace(request.SubscriberId))
            return AdapterResult.Fail("SubscriberId is required", adapterName: AdapterName);

        if (string.IsNullOrWhiteSpace(request.Reason))
            return AdapterResult.Fail("Termination reason is required", adapterName: AdapterName);

        if (request.Immediate)
        {
            _logger.LogWarning("[SIM] Immediate termination requested for {Subscriber} - requires operator approval", request.SubscriberId);
            return AdapterResult.Blocked(
                "Immediate termination requires operator approval. Schedule termination instead.",
                Guid.NewGuid().ToString("N"),
                AdapterName);
        }

        await Task.Delay(_random.Next(100, 300), cancellationToken);

        var response = new TerminateResponse(
            TerminationDate: DateTime.UtcNow.AddDays(7),
            CircuitReleased: true,
            Status: "scheduled-for-termination");

        return AdapterResult.Simulated(
            JsonSerializer.Serialize(response),
            TimeSpan.FromMilliseconds(200),
            AdapterName);
    }

    public async Task<AdapterResult> GetDeviceStatusAsync(DeviceStatusRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[SIM] GetDeviceStatus: Device={Device}, Type={Type}",
            request.DeviceHostname, request.DeviceType);

        if (string.IsNullOrWhiteSpace(request.DeviceHostname))
            return AdapterResult.Fail("Device hostname is required", adapterName: AdapterName);

        await Task.Delay(_random.Next(50, 150), cancellationToken);

        var response = new DeviceStatusResponse(
            Status: "online",
            Uptime: TimeSpan.FromDays(_random.Next(1, 365)),
            CpuUsagePercent: Math.Round(_random.NextDouble() * 80 + 5, 1),
            MemoryUsagePercent: Math.Round(_random.NextDouble() * 60 + 20, 1),
            TemperatureCelsius: Math.Round(_random.NextDouble() * 20 + 35, 1),
            ActivePortCount: _random.Next(10, 48),
            TotalPortCount: 48);

        return AdapterResult.Simulated(
            JsonSerializer.Serialize(response),
            TimeSpan.FromMilliseconds(100),
            AdapterName);
    }

    public async Task<AdapterResult> GetAlarmsAsync(AlarmQueryRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[SIM] GetAlarms: Device={Device}, Severity={Severity}",
            request.DeviceHostname, request.Severity);

        if (string.IsNullOrWhiteSpace(request.DeviceHostname))
            return AdapterResult.Fail("Device hostname is required", adapterName: AdapterName);

        await Task.Delay(_random.Next(80, 200), cancellationToken);

        var alarms = GenerateSimulatedAlarms(request.Severity, request.AlarmType, 5);
        var response = new AlarmQueryResponse(alarms, alarms.Count);

        return AdapterResult.Simulated(
            JsonSerializer.Serialize(response),
            TimeSpan.FromMilliseconds(150),
            AdapterName);
    }

    public async Task<AdapterResult> CollectPerformanceMetricsAsync(MetricsRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[SIM] CollectMetrics: Device={Device}, MetricType={MetricType}, Interval={Interval}",
            request.DeviceHostname, request.MetricType, request.Interval);

        if (string.IsNullOrWhiteSpace(request.DeviceHostname))
            return AdapterResult.Fail("Device hostname is required", adapterName: AdapterName);

        if (string.IsNullOrWhiteSpace(request.MetricType))
            return AdapterResult.Fail("Metric type is required", adapterName: AdapterName);

        var validIntervals = new[] { "5m", "15m", "1h", "24h" };
        if (!validIntervals.Contains(request.Interval))
            return AdapterResult.Fail($"Invalid interval: {request.Interval}. Must be one of: {string.Join(", ", validIntervals)}", adapterName: AdapterName);

        await Task.Delay(_random.Next(100, 300), cancellationToken);

        var metrics = new Dictionary<string, double>
        {
            ["rx_bps"] = Math.Round(_random.NextDouble() * 1_000_000_000, 0),
            ["tx_bps"] = Math.Round(_random.NextDouble() * 500_000_000, 0),
            ["rx_packets"] = Math.Round(_random.NextDouble() * 10_000_000, 0),
            ["tx_packets"] = Math.Round(_random.NextDouble() * 5_000_000, 0),
            ["rx_errors"] = Math.Round(_random.NextDouble() * 100, 0),
            ["tx_errors"] = Math.Round(_random.NextDouble() * 50, 0),
            ["rx_discards"] = Math.Round(_random.NextDouble() * 200, 0),
            ["tx_discards"] = Math.Round(_random.NextDouble() * 100, 0),
        };

        var response = new MetricsResponse(
            request.DeviceHostname,
            request.MetricType,
            request.Interval,
            metrics,
            DateTime.UtcNow);

        return AdapterResult.Simulated(
            JsonSerializer.Serialize(response),
            TimeSpan.FromMilliseconds(200),
            AdapterName);
    }

    public async Task<AdapterResult> ReconcileInventoryAsync(ReconcileRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[SIM] ReconcileInventory: Device={Device}, ResourceType={ResourceType}",
            request.DeviceHostname, request.ResourceType);

        if (string.IsNullOrWhiteSpace(request.DeviceHostname))
            return AdapterResult.Fail("Device hostname is required", adapterName: AdapterName);

        if (string.IsNullOrWhiteSpace(request.ResourceType))
            return AdapterResult.Fail("Resource type is required", adapterName: AdapterName);

        await Task.Delay(_random.Next(200, 500), cancellationToken);

        var resources = Enumerable.Range(1, 10).Select(i => new Dictionary<string, string>
        {
            ["id"] = Guid.NewGuid().ToString("N"),
            ["name"] = $"{request.ResourceType}-{i}",
            ["status"] = i % 3 == 0 ? "fault" : "active",
            ["location"] = $"{_random.Next(1, 10)}/{_random.Next(1, 10)}/{i}"
        }).ToList();

        var response = new ReconcileResponse(
            request.DeviceHostname,
            request.ResourceType,
            resources.Count,
            resources,
            DateTime.UtcNow);

        return AdapterResult.Simulated(
            JsonSerializer.Serialize(response),
            TimeSpan.FromMilliseconds(400),
            AdapterName);
    }

    private static string GenerateOntId()
    {
        return $"ONT-{_random.Next(100000, 999999)}";
    }

    private static List<AlarmEntry> GenerateSimulatedAlarms(string? severityFilter, string? alarmTypeFilter, int count)
    {
        var severities = new[] { "critical", "major", "minor", "warning" };
        var alarmTypes = new[] { "linkDown", "portFailure", "signalDegrade", "powerAlarm", "temperatureAlarm" };
        var descriptions = new Dictionary<string, string>
        {
            ["linkDown"] = "Optical link signal loss detected",
            ["portFailure"] = "Port hardware failure detected",
            ["signalDegrade"] = "Optical signal degradation below threshold",
            ["powerAlarm"] = "Power supply voltage out of range",
            ["temperatureAlarm"] = "Device temperature exceeds warning threshold",
        };

        var alarms = new List<AlarmEntry>();
        for (var i = 0; i < count; i++)
        {
            var severity = severities[_random.Next(severities.Length)];
            var alarmType = alarmTypes[_random.Next(alarmTypes.Length)];

            if (severityFilter is not null && !severity.Equals(severityFilter, StringComparison.OrdinalIgnoreCase))
                continue;

            if (alarmTypeFilter is not null && !alarmType.Equals(alarmTypeFilter, StringComparison.OrdinalIgnoreCase))
                continue;

            alarms.Add(new AlarmEntry(
                AlarmId: Guid.NewGuid().ToString("N"),
                Severity: severity,
                AlarmType: alarmType,
                Description: descriptions.GetValueOrDefault(alarmType, "Unknown alarm"),
                RaisedAt: DateTime.UtcNow.AddHours(-_random.Next(1, 72)),
                Acknowledged: _random.Next(2) == 0));
        }

        return alarms;
    }

    [GeneratedRegex(@"^HWTC[0-9A-Fa-f]{8}$")]
    private static partial Regex OntSerialRegex();

    [GeneratedRegex(@"^\d{15}$")]
    private static partial Regex ImsiRegex();

    [GeneratedRegex(@"^\d{10,15}$")]
    private static partial Regex MsisdnRegex();
}
