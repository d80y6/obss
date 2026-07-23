using System.Text.Json;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;
using Obss.Provisioning.Infrastructure.Adapters.Common;

namespace Obss.Provisioning.Infrastructure.Adapters.Cisco;

public sealed class CiscoProvisioningAdapter : IProvisioningAdapter
{
    private readonly ICiscoRouterAdapter _adapter;
    private readonly ILogger<CiscoProvisioningAdapter> _logger;

    public CiscoProvisioningAdapter(ICiscoRouterAdapter adapter, ILogger<CiscoProvisioningAdapter> logger)
    {
        _adapter = adapter;
        _logger = logger;
    }

    public string AdapterName => CiscoAdapterConstants.AdapterName;

    public async Task<ProvisioningResult> ExecuteAsync(ProvisioningTask task, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation("CiscoProvisioningAdapter executing task {TaskId} of type {TaskType}",
            task.Id, task.TaskType);

        var config = GetConfig(task);

        try
        {
            return task.TaskType switch
            {
                ProvisioningTaskType.RouterInterfaceConfig => await HandleConfigureInterface(config),
                ProvisioningTaskType.RouterBgpConfig => await HandleConfigureBgp(config),
                ProvisioningTaskType.RouterOspfConfig => await HandleConfigureOspf(config),
                ProvisioningTaskType.RouterStaticRouteConfig => await HandleConfigureStaticRoute(config),
                ProvisioningTaskType.RouterSystemConfig => await HandleConfigureSystem(config),
                ProvisioningTaskType.RouterAclConfig => await HandleConfigureAcl(config),
                ProvisioningTaskType.GetRouterStatus => await HandleGetDeviceStatus(),
                ProvisioningTaskType.GetRouterInventory => await HandleGetInventory(),
                ProvisioningTaskType.GetRouterAlarms => await HandleGetActiveAlarms(),
                _ => ProvisioningResult.Fail($"Unsupported task type for Cisco adapter: {task.TaskType}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cisco adapter failed for task {TaskId}", task.Id);
            return ProvisioningResult.Fail(ex.Message);
        }
    }

    public async Task<ProvisioningResult> CompensateAsync(ProvisioningTask task, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation("CiscoProvisioningAdapter compensating task {TaskId} of type {TaskType}",
            task.Id, task.TaskType);

        try
        {
            return task.TaskType switch
            {
                ProvisioningTaskType.RouterInterfaceConfig => await HandleCompensateInterface(task),
                ProvisioningTaskType.RouterStaticRouteConfig => await HandleCompensateStaticRoute(task),
                _ => ProvisioningResult.Ok(JsonSerializer.Serialize(new { compensated = true }))
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cisco adapter compensation failed for task {TaskId}", task.Id);
            return ProvisioningResult.Fail(ex.Message);
        }
    }

    private async Task<ProvisioningResult> HandleConfigureInterface(JsonElement config)
    {
        var ifaceConfig = MapToInterfaceConfig(config);
        var result = await _adapter.ConfigureInterfaceAsync(ifaceConfig);
        return MapResult(result);
    }

    private async Task<ProvisioningResult> HandleConfigureBgp(JsonElement config)
    {
        var bgpConfig = MapToBgpConfig(config);
        var result = await _adapter.ConfigureBgpAsync(bgpConfig);
        return MapResult(result);
    }

    private async Task<ProvisioningResult> HandleConfigureOspf(JsonElement config)
    {
        var ospfConfig = MapToOspfConfig(config);
        var result = await _adapter.ConfigureOspfAsync(ospfConfig);
        return MapResult(result);
    }

    private async Task<ProvisioningResult> HandleConfigureStaticRoute(JsonElement config)
    {
        var route = MapToStaticRoute(config);
        var result = await _adapter.ConfigureStaticRouteAsync(route);
        return MapResult(result);
    }

    private async Task<ProvisioningResult> HandleConfigureSystem(JsonElement config)
    {
        var sysConfig = MapToSystemConfig(config);
        var result = await _adapter.ConfigureSystemAsync(sysConfig);
        return MapResult(result);
    }

    private async Task<ProvisioningResult> HandleConfigureAcl(JsonElement config)
    {
        var aclConfig = MapToAclConfig(config);
        var result = await _adapter.ConfigureAclAsync(aclConfig);
        return MapResult(result);
    }

    private async Task<ProvisioningResult> HandleGetDeviceStatus()
    {
        var result = await _adapter.GetDeviceStatusAsync();
        return MapResult(result);
    }

    private async Task<ProvisioningResult> HandleGetInventory()
    {
        var result = await _adapter.GetInventoryAsync();
        return MapResult(result);
    }

    private async Task<ProvisioningResult> HandleGetActiveAlarms()
    {
        var result = await _adapter.GetActiveAlarmsAsync();
        return MapResult(result);
    }

    private async Task<ProvisioningResult> HandleCompensateInterface(ProvisioningTask task)
    {
        var config = GetConfig(task);
        var ifaceName = ExtractConfigString(config, "interfaceName", "");
        if (string.IsNullOrEmpty(ifaceName))
            return ProvisioningResult.Fail("Interface name required for compensation");

        var result = await _adapter.DeleteInterfaceAsync(ifaceName);
        return MapResult(result);
    }

    private async Task<ProvisioningResult> HandleCompensateStaticRoute(ProvisioningTask task)
    {
        var config = GetConfig(task);
        var prefix = ExtractConfigString(config, "prefix", "");
        if (string.IsNullOrEmpty(prefix))
            return ProvisioningResult.Fail("Route prefix required for compensation");

        var route = new StaticRoute(prefix, "", null, null);
        var result = await _adapter.ConfigureStaticRouteAsync(route);
        return MapResult(result);
    }

    private static JsonElement GetConfig(ProvisioningTask task)
    {
        return task.Configuration is not null
            ? JsonSerializer.Deserialize<JsonElement>(task.Configuration.RootElement.GetRawText())
            : new JsonElement();
    }

    private static string ExtractConfigString(JsonElement config, string key, string defaultValue)
        => config.TryGetProperty(key, out var p) ? p.GetString() ?? defaultValue : defaultValue;

    private static int ExtractConfigInt(JsonElement config, string key, int defaultValue)
        => config.TryGetProperty(key, out var p) ? p.GetInt32() : defaultValue;

    private static string? ExtractConfigStringOrNull(JsonElement config, string key)
        => config.TryGetProperty(key, out var p) ? p.GetString() : null;

    private static int? ExtractConfigIntOrNull(JsonElement config, string key)
        => config.TryGetProperty(key, out var p) && p.TryGetInt32(out var val) ? val : null;

    private static bool? ExtractConfigBoolOrNull(JsonElement config, string key)
        => config.TryGetProperty(key, out var p) ? p.GetBoolean() : null;

    private static ProvisioningResult MapResult<T>(AdapterResult<T> result)
    {
        if (result.IsSuccess)
            return ProvisioningResult.Ok(JsonSerializer.Serialize(result.Data));

        return ProvisioningResult.Fail(result.ErrorMessage ?? "Unknown error");
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

    private static InterfaceConfig MapToInterfaceConfig(JsonElement config)
    {
        var vlanId = ExtractConfigStringOrNull(config, "vlanId");
        return new InterfaceConfig(
            ExtractConfigString(config, "name", ""),
            ExtractConfigString(config, "type", "GigabitEthernet"),
            ExtractConfigStringOrNull(config, "description"),
            ExtractConfigStringOrNull(config, "ipAddress"),
            ExtractConfigIntOrNull(config, "prefixLength"),
            ExtractConfigBoolOrNull(config, "adminUp"),
            ExtractConfigIntOrNull(config, "mtu"),
            vlanId,
            null
        );
    }

    private static BgpConfig MapToBgpConfig(JsonElement config)
    {
        return new BgpConfig(
            ExtractConfigInt(config, "asNumber", 0),
            ExtractConfigStringOrNull(config, "routerId"),
            null,
            null
        );
    }

    private static OspfConfig MapToOspfConfig(JsonElement config)
    {
        return new OspfConfig(
            ExtractConfigInt(config, "processId", 0),
            ExtractConfigStringOrNull(config, "routerId"),
            null
        );
    }

    private static StaticRoute MapToStaticRoute(JsonElement config)
    {
        return new StaticRoute(
            ExtractConfigString(config, "prefix", ""),
            ExtractConfigString(config, "nextHop", ""),
            ExtractConfigIntOrNull(config, "administrativeDistance"),
            ExtractConfigStringOrNull(config, "interface")
        );
    }

    private static SystemConfig MapToSystemConfig(JsonElement config)
    {
        return new SystemConfig(
            ExtractConfigStringOrNull(config, "hostname"),
            ExtractConfigStringOrNull(config, "domainName"),
            null,
            null,
            null
        );
    }

    private static AclConfig MapToAclConfig(JsonElement config)
    {
        return new AclConfig(
            ExtractConfigString(config, "name", ""),
            Array.Empty<AclEntry>()
        );
    }
}
