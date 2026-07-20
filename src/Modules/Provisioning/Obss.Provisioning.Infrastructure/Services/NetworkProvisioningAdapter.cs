using System.Text.Json;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;

namespace Obss.Provisioning.Infrastructure.Services;

public sealed class NetworkProvisioningAdapter : IProvisioningAdapter
{
    private readonly ILogger<NetworkProvisioningAdapter> _logger;
    private readonly string _subnetMask;

    public NetworkProvisioningAdapter(ILogger<NetworkProvisioningAdapter> logger, string subnetMask = "255.255.255.0")
    {
        _logger = logger;
        _subnetMask = subnetMask;
    }

    public string AdapterName => "NetworkAdapter";

    public async Task<ProvisioningResult> ExecuteAsync(ProvisioningTask task, CancellationToken cancellationToken)
    {
        _logger.LogInformation("NetworkProvisioningAdapter executing task {TaskId} of type {TaskType}",
            task.Id, task.TaskType);

        return task.TaskType switch
        {
            ProvisioningTaskType.NetworkConfig => await ConfigureNetworkAsync(task),
            ProvisioningTaskType.ResourceAllocation => await AllocateResourcesAsync(task),
            _ => ProvisioningResult.Fail($"Unsupported task type for NetworkAdapter: {task.TaskType}")
        };
    }

    public async Task<ProvisioningResult> CompensateAsync(ProvisioningTask task, CancellationToken cancellationToken)
    {
        _logger.LogInformation("NetworkProvisioningAdapter compensating task {TaskId} of type {TaskType}",
            task.Id, task.TaskType);

        return task.TaskType switch
        {
            ProvisioningTaskType.NetworkConfig => await RollbackNetworkConfigAsync(task),
            ProvisioningTaskType.ResourceAllocation => await ReleaseResourcesAsync(task),
            _ => ProvisioningResult.Ok(JsonSerializer.Serialize(new { compensated = true }))
        };
    }

    private Task<ProvisioningResult> ConfigureNetworkAsync(ProvisioningTask task)
    {
        var config = task.Configuration is not null
            ? JsonSerializer.Deserialize<JsonElement>(task.Configuration!.RootElement.GetRawText())
            : new JsonElement();

        var serviceType = config.TryGetProperty("serviceType", out var st) ? st.GetString() : "unknown";
        var speed = config.TryGetProperty("speedMbps", out var sp) ? sp.GetInt32() : 100;

        _logger.LogInformation("Configuring network for {ServiceType} at {Speed} Mbps - vendor confirmation required", serviceType, speed);

        return Task.FromResult(ProvisioningResult.Blocked(
            $"Network configuration for {serviceType} at {speed} Mbps requires vendor confirmation - adapter cannot complete without real network element",
            TimeSpan.FromMilliseconds(150)));
    }

    private Task<ProvisioningResult> AllocateResourcesAsync(ProvisioningTask task)
    {
        var config = task.Configuration is not null
            ? JsonSerializer.Deserialize<JsonElement>(task.Configuration!.RootElement.GetRawText())
            : new JsonElement();

        var bandwidth = config.TryGetProperty("bandwidthMbps", out var bw) ? bw.GetInt32() : 1000;
        var ipCount = config.TryGetProperty("ipAddresses", out var ipc) ? ipc.GetInt32() : 1;

        _logger.LogInformation("Resource allocation requires vendor confirmation: {Bandwidth} Mbps, {IPCount} IPs", bandwidth, ipCount);

        return Task.FromResult(ProvisioningResult.Blocked(
            $"Resource allocation ({bandwidth} Mbps, {ipCount} IPs, subnet {_subnetMask}) requires vendor network element confirmation",
            TimeSpan.FromMilliseconds(200)));
    }

    private Task<ProvisioningResult> RollbackNetworkConfigAsync(ProvisioningTask task)
    {
        _logger.LogInformation("Rolling back network configuration for task {TaskId}", task.Id);
        return Task.FromResult(ProvisioningResult.Ok(
            JsonSerializer.Serialize(new { reverted = true, configRemoved = true }),
            TimeSpan.FromMilliseconds(50)));
    }

    private Task<ProvisioningResult> ReleaseResourcesAsync(ProvisioningTask task)
    {
        _logger.LogInformation("Releasing resources for task {TaskId}", task.Id);
        return Task.FromResult(ProvisioningResult.Ok(
            JsonSerializer.Serialize(new { deallocated = true, ipsReleased = true }),
            TimeSpan.FromMilliseconds(30)));
    }
}
