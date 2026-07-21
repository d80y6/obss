using System.Text.Json;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.Provisioning.Infrastructure.Adapters.Common;
using Obss.Provisioning.Infrastructure.Adapters.Huawei;

namespace Obss.Provisioning.Infrastructure.Services;

public sealed class HuaweiProvisioningAdapter : IProvisioningAdapter
{
    private readonly IHuaweiBroadbandAdapter _huaweiAdapter;
    private readonly ILogger<HuaweiProvisioningAdapter> _logger;

    public HuaweiProvisioningAdapter(IHuaweiBroadbandAdapter huaweiAdapter, ILogger<HuaweiProvisioningAdapter> logger)
    {
        _huaweiAdapter = huaweiAdapter;
        _logger = logger;
    }

    public string AdapterName => AdapterConstants.AdapterNames.HuaweiBroadband;

    public async Task<ProvisioningResult> ExecuteAsync(ProvisioningTask task, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HuaweiProvisioningAdapter executing task {TaskId} of type {TaskType}",
            task.Id, task.TaskType);

        var config = GetConfig(task);

        return task.TaskType switch
        {
            ProvisioningTaskType.FtthOntProvision or ProvisioningTaskType.FtthServicePortConfig
                => await HandleFtthActivation(config, cancellationToken),

            ProvisioningTaskType.FtthVlanConfig
                => await HandleChangeService(config, cancellationToken),

            ProvisioningTaskType.FtthPppoeConfig
                => ProvisioningResult.Ok(JsonSerializer.Serialize(new { configured = true, pppoe = true })),

            ProvisioningTaskType.AdslDslPortConfig or ProvisioningTaskType.AdslLineProfileConfig
                => await HandleAdslActivation(config, cancellationToken),

            ProvisioningTaskType.LteSubscriberActivation or ProvisioningTaskType.LteApnConfig
                => await HandleLteActivation(config, cancellationToken),

            ProvisioningTaskType.WifiAccessConfig
                => await HandleWifiActivation(config, cancellationToken),

            ProvisioningTaskType.NetworkConfig or ProvisioningTaskType.ResourceAllocation
                or ProvisioningTaskType.DiaCircuitConfig or ProvisioningTaskType.EthernetCircuitConfig
                or ProvisioningTaskType.StaticIpAllocation
                => ProvisioningResult.Blocked(
                    "Huawei adapter: network-level provisioning requires operator intervention for L2/L3 services"),

            _ => ProvisioningResult.Fail($"Unsupported task type for Huawei adapter: {task.TaskType}")
        };
    }

    public async Task<ProvisioningResult> CompensateAsync(ProvisioningTask task, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HuaweiProvisioningAdapter compensating task {TaskId} of type {TaskType}",
            task.Id, task.TaskType);

        var config = GetConfig(task);
        var subscriberId = ExtractSubscriberId(config);

        return task.TaskType switch
        {
            ProvisioningTaskType.FtthOntProvision or ProvisioningTaskType.FtthServicePortConfig
                or ProvisioningTaskType.AdslDslPortConfig or ProvisioningTaskType.AdslLineProfileConfig
                => await CompensateServiceActivation(subscriberId, cancellationToken),

            _ => ProvisioningResult.Ok(JsonSerializer.Serialize(new { compensated = true }))
        };
    }

    private async Task<ProvisioningResult> HandleFtthActivation(JsonElement config, CancellationToken cancellationToken)
    {
        var subscriberId = ExtractSubscriberId(config);
        var ontSerial = ExtractConfigString(config, "ontSerial", "HWTC00000001");
        var oltHostname = ExtractConfigString(config, "oltHostname", "");
        var ponPort = ExtractConfigString(config, "ponPort", "0/1");
        var vlanId = ExtractConfigInt(config, "vlanId", 100);
        var bandwidth = ExtractConfigInt(config, "bandwidthMbps", 100);
        var pppoeUsername = ExtractConfigOrNull(config, "pppoeUsername");
        var pppoePassword = ExtractConfigOrNull(config, "pppoePassword");

        var request = new ActivateFtthRequest(
            subscriberId, ontSerial, oltHostname, ponPort,
            vlanId, pppoeUsername, pppoePassword, bandwidth, vlanId);

        var result = await _huaweiAdapter.ActivateFtthAsync(request, cancellationToken);
        return MapResult(result);
    }

    private async Task<ProvisioningResult> HandleAdslActivation(JsonElement config, CancellationToken cancellationToken)
    {
        var subscriberId = ExtractSubscriberId(config);
        var dslamHostname = ExtractConfigString(config, "dslamHostname", "");
        var dslamPort = ExtractConfigString(config, "dslamPort", "0/1");

        var request = new ActivateAdslRequest(subscriberId, dslamHostname, dslamPort, "default", 8, 35, null, null);
        var result = await _huaweiAdapter.ActivateAdslAsync(request, cancellationToken);
        return MapResult(result);
    }

    private async Task<ProvisioningResult> HandleLteActivation(JsonElement config, CancellationToken cancellationToken)
    {
        var subscriberId = ExtractSubscriberId(config);
        var imsi = ExtractConfigString(config, "imsi", "");
        var msisdn = ExtractConfigString(config, "msisdn", "");

        var request = new Activate4GRequest(subscriberId, imsi, msisdn, "internet", "default", "10GB", "default");
        var result = await _huaweiAdapter.Activate4GAsync(request, cancellationToken);
        return MapResult(result);
    }

    private async Task<ProvisioningResult> HandleWifiActivation(JsonElement config, CancellationToken cancellationToken)
    {
        var ssid = ExtractConfigString(config, "ssid", "");
        var passphrase = ExtractConfigString(config, "passphrase", "");
        var encryption = ExtractConfigString(config, "encryption", "WPA2");

        var request = new ActivateWiFiRequest("", ssid, passphrase, encryption, "dual", 10);
        var result = await _huaweiAdapter.ActivateWiFiAsync(request, cancellationToken);
        return MapResult(result);
    }

    private async Task<ProvisioningResult> HandleChangeService(JsonElement config, CancellationToken cancellationToken)
    {
        var subscriberId = ExtractSubscriberId(config);
        var newBandwidth = config.TryGetProperty("bandwidthMbps", out var bwProp)
            ? bwProp.GetInt32()
            : (int?)null;

        var request = new ChangeServiceRequest(subscriberId, "ftth", newBandwidth, null, null);
        var result = await _huaweiAdapter.ChangeServiceAsync(request, cancellationToken);
        return MapResult(result);
    }

    private async Task<ProvisioningResult> CompensateServiceActivation(string subscriberId, CancellationToken cancellationToken)
    {
        var request = new TerminateRequest(subscriberId, "ftth", "rollback", false);
        var result = await _huaweiAdapter.TerminateServiceAsync(request, cancellationToken);
        return MapResult(result);
    }

    private static ProvisioningResult MapResult(AdapterResult result)
    {
        return result.State switch
        {
            AdapterOperationState.Success => ProvisioningResult.Ok(result.ResultData, result.Duration),
            AdapterOperationState.Failed => ProvisioningResult.Fail(result.ErrorMessage ?? "Unknown error", result.Duration),
            AdapterOperationState.BlockedNeedsOperator => ProvisioningResult.Blocked(result.ErrorMessage ?? "Blocked", result.Duration),
            AdapterOperationState.PendingVendorConfirmation => ProvisioningResult.PendingConfirmation(result.ResultData, result.Duration),
            AdapterOperationState.Simulated => ProvisioningResult.Ok(result.ResultData, result.Duration),
            _ => ProvisioningResult.Fail($"Unknown adapter state: {result.State}")
        };
    }

    private static JsonElement GetConfig(ProvisioningTask task)
    {
        return task.Configuration is not null
            ? JsonSerializer.Deserialize<JsonElement>(task.Configuration.RootElement.GetRawText())
            : new JsonElement();
    }

    private static string ExtractSubscriberId(JsonElement config)
        => config.TryGetProperty("subscriberId", out var p) ? p.GetString() ?? "" : "";

    private static string ExtractConfigString(JsonElement config, string key, string defaultValue)
        => config.TryGetProperty(key, out var p) ? p.GetString() ?? defaultValue : defaultValue;

    private static int ExtractConfigInt(JsonElement config, string key, int defaultValue)
        => config.TryGetProperty(key, out var p) ? p.GetInt32() : defaultValue;

    private static string? ExtractConfigOrNull(JsonElement config, string key)
        => config.TryGetProperty(key, out var p) ? p.GetString() : null;
}
