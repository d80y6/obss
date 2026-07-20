using Obss.Provisioning.Infrastructure.Adapters.Common;

namespace Obss.Provisioning.Infrastructure.Adapters.Huawei;

public sealed record ActivateFtthRequest(
    string SubscriberId,
    string OntSerial,
    string OltHostname,
    string PonPort,
    int VlanId,
    string? PppoeUsername,
    string? PppoePassword,
    int BandwidthMbps,
    int ServiceVlan);

public sealed record ActivateFtthResponse(
    string OntId,
    string CircuitId,
    string Status,
    DateTime ActivationTimestamp);

public sealed record ActivateAdslRequest(
    string SubscriberId,
    string DslamHostname,
    string DslamPort,
    string LineProfile,
    int Vpi,
    int Vci,
    string? PppoeUsername,
    string? PppoePassword);

public sealed record ActivateAdslResponse(
    string DslamPortId,
    string CircuitId,
    string Status,
    DateTime ActivationTimestamp);

public sealed record Activate4GRequest(
    string SubscriberId,
    string Imsi,
    string Msisdn,
    string ApnName,
    string QosProfile,
    string DataPlan,
    string PolicyProfile);

public sealed record Activate4GResponse(
    string NetworkSubscriberId,
    string PdnId,
    string BearerId,
    string Status,
    DateTime ActivationTimestamp);

public sealed record ActivateWiFiRequest(
    string SubscriberId,
    string Ssid,
    string Passphrase,
    string Encryption,
    string RadioBand,
    int ClientLimit);

public sealed record ActivateWiFiResponse(
    string WifiProfileId,
    string Status,
    DateTime ActivationTimestamp);

public sealed record SuspendRequest(
    string SubscriberId,
    string ServiceType,
    string Reason);

public sealed record SuspendResponse(
    DateTime SuspensionDate,
    DateTime? ExpectedResumeDate,
    string Status);

public sealed record ResumeRequest(
    string SubscriberId,
    string ServiceType);

public sealed record ResumeResponse(
    DateTime ResumeDate,
    string Status);

public sealed record ChangeServiceRequest(
    string SubscriberId,
    string ServiceType,
    int? NewBandwidthMbps,
    string? NewProfile,
    int? NewVlanId);

public sealed record ChangeServiceResponse(
    DateTime ChangeDate,
    string OldSpeed,
    string NewSpeed,
    string Status);

public sealed record TerminateRequest(
    string SubscriberId,
    string ServiceType,
    string Reason,
    bool Immediate);

public sealed record TerminateResponse(
    DateTime TerminationDate,
    bool CircuitReleased,
    string Status);

public sealed record DeviceStatusRequest(
    string DeviceHostname,
    string DeviceType);

public sealed record DeviceStatusResponse(
    string Status,
    TimeSpan Uptime,
    double CpuUsagePercent,
    double MemoryUsagePercent,
    double TemperatureCelsius,
    int ActivePortCount,
    int TotalPortCount);

public sealed record AlarmQueryRequest(
    string DeviceHostname,
    string? Severity,
    DateTime? StartTime,
    DateTime? EndTime,
    string? AlarmType);

public sealed record AlarmEntry(
    string AlarmId,
    string Severity,
    string AlarmType,
    string Description,
    DateTime RaisedAt,
    bool Acknowledged);

public sealed record AlarmQueryResponse(
    IReadOnlyCollection<AlarmEntry> Alarms,
    int TotalCount);

public sealed record MetricsRequest(
    string DeviceHostname,
    string MetricType,
    string Interval);

public sealed record MetricsResponse(
    string DeviceHostname,
    string MetricType,
    string Interval,
    IReadOnlyDictionary<string, double> Metrics,
    DateTime CollectedAt);

public sealed record ReconcileRequest(
    string DeviceHostname,
    string ResourceType);

public sealed record ReconcileResponse(
    string DeviceHostname,
    string ResourceType,
    int TotalResources,
    IReadOnlyCollection<Dictionary<string, string>> Resources,
    DateTime ReconcileTimestamp);

public interface IHuaweiBroadbandAdapter
{
    string AdapterName { get; }

    string TechnologyType { get; }

    Task<AdapterResult> ActivateFtthAsync(ActivateFtthRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult> ActivateAdslAsync(ActivateAdslRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult> Activate4GAsync(Activate4GRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult> ActivateWiFiAsync(ActivateWiFiRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult> SuspendServiceAsync(SuspendRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult> ResumeServiceAsync(ResumeRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult> ChangeServiceAsync(ChangeServiceRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult> TerminateServiceAsync(TerminateRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult> GetDeviceStatusAsync(DeviceStatusRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult> GetAlarmsAsync(AlarmQueryRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult> CollectPerformanceMetricsAsync(MetricsRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult> ReconcileInventoryAsync(ReconcileRequest request, CancellationToken cancellationToken = default);
}
