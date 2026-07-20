using System.Text.Json;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;

namespace Obss.Provisioning.Infrastructure.Services;

public sealed class TestProvisioningAdapter : IProvisioningAdapter
{
    private readonly ILogger<TestProvisioningAdapter> _logger;

    public TestProvisioningAdapter(ILogger<TestProvisioningAdapter> logger)
    {
        _logger = logger;
    }

    public string AdapterName => "TestAdapter";

    public Task<ProvisioningResult> ExecuteAsync(ProvisioningTask task, CancellationToken cancellationToken)
    {
        _logger.LogInformation("TestAdapter executing task {TaskId} of type {TaskType}", task.Id, task.TaskType);

        var result = task.TaskType switch
        {
            ProvisioningTaskType.NetworkConfig => ProvisioningResult.Ok(
                JsonSerializer.Serialize(new { configured = true, config = "default" })),
            ProvisioningTaskType.ResourceAllocation => ProvisioningResult.Ok(
                JsonSerializer.Serialize(new { allocated = true, resourceId = Guid.NewGuid() })),
            ProvisioningTaskType.AccountSetup => ProvisioningResult.Ok(
                JsonSerializer.Serialize(new { accountCreated = true, accountId = Guid.NewGuid() })),
            ProvisioningTaskType.EmailNotification => ProvisioningResult.Ok(
                JsonSerializer.Serialize(new { notified = true, channel = "email" })),
            ProvisioningTaskType.PhysicalInstall => ProvisioningResult.Ok(
                JsonSerializer.Serialize(new { installed = true })),
            ProvisioningTaskType.DNSSetup => ProvisioningResult.Ok(
                JsonSerializer.Serialize(new { dnsConfigured = true })),
            ProvisioningTaskType.Custom => ProvisioningResult.Ok(
                JsonSerializer.Serialize(new { customTask = true, processed = true })),
            _ => ProvisioningResult.Fail($"Unknown task type: {task.TaskType}")
        };

        return Task.FromResult(result);
    }

    public Task<ProvisioningResult> CompensateAsync(ProvisioningTask task, CancellationToken cancellationToken)
    {
        _logger.LogInformation("TestAdapter compensating task {TaskId} of type {TaskType}", task.Id, task.TaskType);

        var result = task.TaskType switch
        {
            ProvisioningTaskType.ResourceAllocation => ProvisioningResult.Ok(
                JsonSerializer.Serialize(new { deallocated = true })),
            ProvisioningTaskType.NetworkConfig => ProvisioningResult.Ok(
                JsonSerializer.Serialize(new { reverted = true })),
            ProvisioningTaskType.AccountSetup => ProvisioningResult.Ok(
                JsonSerializer.Serialize(new { deleted = true })),
            _ => ProvisioningResult.Ok(JsonSerializer.Serialize(new { compensated = true }))
        };

        return Task.FromResult(result);
    }
}
