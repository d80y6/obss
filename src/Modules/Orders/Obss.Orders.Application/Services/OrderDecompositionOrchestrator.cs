using System.Text.Json;

namespace Obss.Orders.Application.Services;

public sealed class OrderDecompositionOrchestrator : IOrderDecompositionOrchestrator
{
    private readonly IFtthOrderDecompositionService _ftth;
    private readonly IAdslOrderDecompositionService _adsl;
    private readonly ILteOrderDecompositionService _lte;
    private readonly ITelephonyOrderDecompositionService _telephony;
    private readonly IBusinessConnectivityDecompositionService _business;
    private readonly IHostingDecompositionService _hosting;

    public OrderDecompositionOrchestrator(
        IFtthOrderDecompositionService ftth,
        IAdslOrderDecompositionService adsl,
        ILteOrderDecompositionService lte,
        ITelephonyOrderDecompositionService telephony,
        IBusinessConnectivityDecompositionService business,
        IHostingDecompositionService hosting)
    {
        _ftth = ftth;
        _adsl = adsl;
        _lte = lte;
        _telephony = telephony;
        _business = business;
        _hosting = hosting;
    }

    public async Task<UnifiedDecompositionResult> DecomposeAsync(UnifiedDecompositionRequest request, CancellationToken ct)
    {
        var serviceType = request.ServiceType.ToUpperInvariant();

        return serviceType switch
        {
            "FTTH" => await DecomposeFtthAsync(request, ct),
            "ADSL" => await DecomposeAdslAsync(request, ct),
            "LTE" or "4G" => await DecomposeLteAsync(request, ct),
            "TELEPHONY" or "POTS" or "VOIP" => await DecomposeTelephonyAsync(request, ct),
            "DIA" or "ETHERNET" or "TDM" or "ETHERNET_CIRCUIT" => await DecomposeBusinessAsync(request, ct),
            "DEDICATED_SERVER" or "VPS" or "COLOCATION" or "HOSTING" => await DecomposeHostingAsync(request, ct),
            _ => throw new ArgumentException($"Unsupported service type: '{request.ServiceType}'")
        };
    }

    private static UnifiedDecompositionResult Merge(
        Guid correlationId,
        IReadOnlyList<ServiceTask> serviceTasks,
        IReadOnlyList<ResourceTask> resourceTasks,
        string adapterType)
    {
        var tasks = new List<DecomposedTask>();

        foreach (var st in serviceTasks)
        {
            tasks.Add(new DecomposedTask(
                st.TaskType,
                adapterType,
                st.TaskName,
                st.TaskNameAr,
                st.StepOrder,
                st.DependsOnTaskType,
                st.Configuration));
        }

        foreach (var rt in resourceTasks)
        {
            tasks.Add(new DecomposedTask(
                rt.TaskType,
                "INTERNAL",
                rt.TaskType,
                rt.TaskType,
                rt.StepOrder,
                rt.DependsOnTaskType,
                rt.Configuration));
        }

        return new UnifiedDecompositionResult(correlationId, tasks.OrderBy(t => t.StepOrder).ToList());
    }

    private async Task<UnifiedDecompositionResult> DecomposeFtthAsync(UnifiedDecompositionRequest request, CancellationToken ct)
    {
        var config = request.ServiceConfiguration;
        var req = new FtthDecompositionRequest(
            request.OrderId,
            request.OrderItemId,
            request.Segment,
            config?.RootElement.TryGetProperty("downloadSpeedMbps", out var d) == true ? d.GetInt32() : 100,
            config?.RootElement.TryGetProperty("uploadSpeedMbps", out var u) == true ? u.GetInt32() : 50,
            config?.RootElement.TryGetProperty("ontSerial", out var s) == true ? s.GetString() : null,
            config?.RootElement.TryGetProperty("loid", out var l) == true ? l.GetString() : null,
            config?.RootElement.TryGetProperty("ppoeUsername", out var p) == true ? p.GetString() : null,
            config?.RootElement.TryGetProperty("installationAddress", out var a) == true ? a.GetString() : null);

        var result = await _ftth.DecomposeAsync(req, ct);
        return Merge(result.CorrelationId, result.ServiceTasks, result.ResourceTasks, "HUAWEI_BROADBAND");
    }

    private async Task<UnifiedDecompositionResult> DecomposeAdslAsync(UnifiedDecompositionRequest request, CancellationToken ct)
    {
        var config = request.ServiceConfiguration;
        var req = new AdslDecompositionRequest(
            request.OrderId,
            request.OrderItemId,
            request.Segment,
            config?.RootElement.TryGetProperty("dslamId", out var d) == true ? d.GetString() : null,
            config?.RootElement.TryGetProperty("vpi", out var vpi) == true ? vpi.GetInt32() : 0,
            config?.RootElement.TryGetProperty("vci", out var vci) == true ? vci.GetInt32() : 35,
            config?.RootElement.TryGetProperty("pppUsername", out var p) == true ? p.GetString() : null,
            config?.RootElement.TryGetProperty("lineProfile", out var l) == true ? l.GetString() : null);

        var result = await _adsl.DecomposeAsync(req, ct);
        return Merge(result.CorrelationId, result.ServiceTasks, result.ResourceTasks, "HUAWEI_BROADBAND");
    }

    private async Task<UnifiedDecompositionResult> DecomposeLteAsync(UnifiedDecompositionRequest request, CancellationToken ct)
    {
        var config = request.ServiceConfiguration;
        var req = new LteDecompositionRequest(
            request.OrderId,
            request.OrderItemId,
            request.Segment,
            config?.RootElement.TryGetProperty("iccid", out var i) == true ? i.GetString() : null,
            config?.RootElement.TryGetProperty("apn", out var a) == true ? a.GetString() : null,
            config?.RootElement.TryGetProperty("dataLimitGb", out var d) == true ? d.GetInt32() : 10);

        var result = await _lte.DecomposeAsync(req, ct);
        return Merge(result.CorrelationId, result.ServiceTasks, result.ResourceTasks, "HUAWEI_BROADBAND");
    }

    private async Task<UnifiedDecompositionResult> DecomposeTelephonyAsync(UnifiedDecompositionRequest request, CancellationToken ct)
    {
        var config = request.ServiceConfiguration;
        var req = new TelephonyDecompositionRequest(
            request.OrderId,
            request.OrderItemId,
            request.Segment,
            config?.RootElement.TryGetProperty("phoneNumber", out var p) == true ? p.GetString() : null,
            config?.RootElement.TryGetProperty("callForwarding", out var cf) == true && cf.GetBoolean(),
            config?.RootElement.TryGetProperty("callWaiting", out var cw) == true && cw.GetBoolean(),
            config?.RootElement.TryGetProperty("callerId", out var ci) == true && ci.GetBoolean(),
            config?.RootElement.TryGetProperty("threeWayCalling", out var tw) == true && tw.GetBoolean());

        var result = await _telephony.DecomposeAsync(req, ct);
        return Merge(result.CorrelationId, result.ServiceTasks, result.ResourceTasks, "ZTE_SOFTSWITCH");
    }

    private async Task<UnifiedDecompositionResult> DecomposeBusinessAsync(UnifiedDecompositionRequest request, CancellationToken ct)
    {
        var config = request.ServiceConfiguration;
        var req = new BusinessConnectivityDecompositionRequest(
            request.OrderId,
            request.OrderItemId,
            request.Segment,
            config?.RootElement.TryGetProperty("connectivityType", out var c) == true ? c.GetString() ?? "DIA" : "DIA",
            config?.RootElement.TryGetProperty("bandwidthMbps", out var b) == true ? b.GetInt32() : 100,
            config?.RootElement.TryGetProperty("vlanId", out var v) == true ? v.GetInt32() : 100,
            config?.RootElement.TryGetProperty("peerIp", out var ip) == true ? ip.GetString() : null,
            config?.RootElement.TryGetProperty("requiresSla", out var sla) == true && sla.GetBoolean());

        var result = await _business.DecomposeAsync(req, ct);
        return Merge(result.CorrelationId, result.ServiceTasks, result.ResourceTasks, "HUAWEI_BROADBAND");
    }

    private async Task<UnifiedDecompositionResult> DecomposeHostingAsync(UnifiedDecompositionRequest request, CancellationToken ct)
    {
        var config = request.ServiceConfiguration;
        var hostingType = DetermineHostingType(request.ServiceType);
        var req = new HostingDecompositionRequest(
            request.OrderId,
            request.OrderItemId,
            request.Segment,
            hostingType,
            config?.RootElement.TryGetProperty("osImage", out var o) == true ? o.GetString() : null,
            config?.RootElement.TryGetProperty("cpuCores", out var cpu) == true ? cpu.GetInt32() : 4,
            config?.RootElement.TryGetProperty("ramGb", out var ram) == true ? ram.GetInt32() : 8,
            config?.RootElement.TryGetProperty("storageGb", out var s) == true ? s.GetInt32() : 100);

        var result = await _hosting.DecomposeAsync(req, ct);
        return Merge(result.CorrelationId, result.ServiceTasks, result.ResourceTasks, "INTERNAL");
    }

    private static string DetermineHostingType(string serviceType)
    {
        return serviceType.ToUpperInvariant() switch
        {
            "DEDICATED_SERVER" => "DedicatedServer",
            "VPS" => "VPS",
            "COLOCATION" => "Colocation",
            _ => "DedicatedServer"
        };
    }
}
