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
            ProvisioningTaskType.NetworkConfig => ProvisioningResult.Blocked(
                "Network configuration requires vendor adapter - TestAdapter cannot complete this operation"),
            ProvisioningTaskType.ResourceAllocation => ProvisioningResult.Blocked(
                "Resource allocation requires vendor adapter - TestAdapter cannot complete this operation"),
            ProvisioningTaskType.AccountSetup => ProvisioningResult.Blocked(
                "Account setup requires billing system - TestAdapter cannot complete this operation"),
            ProvisioningTaskType.DNSSetup => ProvisioningResult.Blocked(
                "DNS setup requires DNS infrastructure adapter - TestAdapter cannot complete this operation"),
            ProvisioningTaskType.PhysicalInstall => ProvisioningResult.Blocked(
                "Physical install requires field operations - TestAdapter cannot complete this operation"),
            ProvisioningTaskType.EmailNotification => ProvisioningResult.Ok(
                JsonSerializer.Serialize(new { notified = true, channel = "email" })),
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
            ProvisioningTaskType.ResourceAllocation => ProvisioningResult.Blocked(
                "Resource deallocation requires vendor adapter - TestAdapter cannot compensate"),
            ProvisioningTaskType.NetworkConfig => ProvisioningResult.Blocked(
                "Network revert requires vendor adapter - TestAdapter cannot compensate"),
            ProvisioningTaskType.AccountSetup => ProvisioningResult.Blocked(
                "Account deletion requires billing system - TestAdapter cannot compensate"),
            _ => ProvisioningResult.Ok(JsonSerializer.Serialize(new { compensated = true }))
        };

        return Task.FromResult(result);
    }
}
