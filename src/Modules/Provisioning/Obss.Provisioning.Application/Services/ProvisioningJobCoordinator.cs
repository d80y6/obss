using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;

namespace Obss.Provisioning.Application.Services;

public sealed class ProvisioningJobCoordinator : IProvisioningJobCoordinator
{
    private readonly IAdapterRegistry _adapterRegistry;
    private readonly IProvisioningJobRepository _jobRepository;
    private readonly ILogger<ProvisioningJobCoordinator> _logger;
    private static readonly ConcurrentDictionary<Guid, CoordinatorProgress> ProgressCache = new();

    private const int DefaultTaskTimeoutSeconds = 300;
    private const int MaxRetries = 3;

    public ProvisioningJobCoordinator(
        IAdapterRegistry adapterRegistry,
        IProvisioningJobRepository jobRepository,
        ILogger<ProvisioningJobCoordinator> logger)
    {
        _adapterRegistry = adapterRegistry;
        _jobRepository = jobRepository;
        _logger = logger;
    }

    public async Task<CoordinatorProgress> ExecuteJobAsync(ProvisioningJob job, CancellationToken ct)
    {
        var orderedTasks = job.Tasks
            .OrderBy(t => t.StepNumber)
            .ToList();

        var completed = new HashSet<Guid>();
        var total = orderedTasks.Count;
        var failedTasks = new List<ProvisioningTask>();

        UpdateProgress(job.Id, 0, total, "InProgress", null, null);

        foreach (var task in orderedTasks)
        {
            if (ct.IsCancellationRequested)
            {
                job.Fail("Job cancelled");
                await _jobRepository.UpdateAsync(job);
                return FailResult(job.Id, completed.Count, total, "Cancelled");
            }

            var dependenciesMet = true;
            foreach (var other in orderedTasks)
            {
                if (other.StepNumber < task.StepNumber && !completed.Contains(other.Id))
                {
                    dependenciesMet = false;
                    break;
                }
            }

            if (!dependenciesMet)
            {
                task.Skip();
                continue;
            }

            var result = await ExecuteTaskWithRetryAsync(task, ct);

            if (result.Success)
            {
                task.Complete();
                completed.Add(task.Id);
                UpdateProgress(job.Id, completed.Count, total, "InProgress", task.TaskType.ToString(), null);
            }
            else
            {
                task.Fail(result.ErrorMessage ?? "Unknown error");
                failedTasks.Add(task);
                job.Fail(result.ErrorMessage ?? $"Task '{task.TaskType}' failed");

                await ExecuteRollbackAsync(job, orderedTasks, completed, ct);

                UpdateProgress(job.Id, completed.Count, total, "RolledBack", task.TaskType.ToString(), result.ErrorMessage);
                await _jobRepository.UpdateAsync(job);
                return FailResult(job.Id, completed.Count, total, "RolledBack", result.ErrorMessage);
            }
        }

        if (failedTasks.Count == 0)
        {
            job.Complete();
            await _jobRepository.UpdateAsync(job);
            UpdateProgress(job.Id, total, total, "Completed", null, null);
            return new CoordinatorProgress(job.Id, total, total, "Completed", null, null);
        }

        await _jobRepository.UpdateAsync(job);
        return FailResult(job.Id, completed.Count, total, "CompletedWithErrors");
    }

    public Task<CoordinatorProgress> GetProgressAsync(Guid jobId, CancellationToken ct)
    {
        if (ProgressCache.TryGetValue(jobId, out var progress))
            return Task.FromResult(progress);

        return Task.FromResult(new CoordinatorProgress(jobId, 0, 0, "Unknown", null, null));
    }

    private async Task<TaskExecutionResult> ExecuteTaskWithRetryAsync(ProvisioningTask task, CancellationToken ct)
    {
        var adapterName = ResolveAdapterName(task.TaskType, task.AssignedTo, task.Configuration);
        var adapter = _adapterRegistry.GetAdapter(adapterName, adapterName);

        if (adapter is null)
        {
            _logger.LogWarning("No adapter found for task type {TaskType} (adapter: {Adapter})", task.TaskType, adapterName);
            return new TaskExecutionResult(task.Id, true, null, TimeSpan.Zero);
        }

        var attempt = 0;
        var timeout = TimeSpan.FromSeconds(DefaultTaskTimeoutSeconds);
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(timeout);

        while (attempt <= MaxRetries)
        {
            try
            {
                task.Start();
                var startTime = DateTime.UtcNow;
                var result = await adapter.ExecuteAsync(task, timeoutCts.Token);
                var duration = DateTime.UtcNow - startTime;

                if (result.Success)
                    return new TaskExecutionResult(task.Id, true, null, duration);

                attempt++;
                if (attempt > MaxRetries)
                {
                    return new TaskExecutionResult(task.Id, false, result.ErrorMessage, duration);
                }

                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                _logger.LogWarning("Task {TaskType} failed (attempt {Attempt}), retrying in {Delay}s",
                    task.TaskType, attempt, delay.TotalSeconds);
                await Task.Delay(delay, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return new TaskExecutionResult(task.Id, false, "Job cancelled", TimeSpan.Zero);
            }
            catch (OperationCanceledException)
            {
                return new TaskExecutionResult(task.Id, false, $"Task timed out after {timeout.TotalSeconds}s", timeout);
            }
            catch (Exception ex)
            {
                attempt++;
                if (attempt > MaxRetries)
                {
                    return new TaskExecutionResult(task.Id, false, ex.Message, TimeSpan.Zero);
                }

                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                await Task.Delay(delay, ct);
            }
        }

        return new TaskExecutionResult(task.Id, false, "Max retries exceeded", TimeSpan.Zero);
    }

    private async Task ExecuteRollbackAsync(
        ProvisioningJob job,
        List<ProvisioningTask> orderedTasks,
        HashSet<Guid> completed,
        CancellationToken ct)
    {
        var rollbackOrder = orderedTasks
            .Where(t => completed.Contains(t.Id))
            .OrderByDescending(t => t.StepNumber)
            .ToList();

        foreach (var task in rollbackOrder)
        {
            try
            {
                var adapterName = ResolveAdapterName(task.TaskType, task.AssignedTo, task.Configuration);
                var adapter = _adapterRegistry.GetAdapter(adapterName, adapterName);

                if (adapter is not null)
                {
                    var result = await adapter.CompensateAsync(task, ct);
                    _logger.LogInformation("Rollback of task {TaskType} completed: {Success}",
                        task.TaskType, result.Success);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rollback failed for task {TaskType}", task.TaskType);
            }
        }

        job.Rollback();
    }

    internal static string ResolveAdapterName(ProvisioningTaskType taskType)
        => ResolveAdapterName(taskType, null, null);

    internal static string ResolveAdapterName(ProvisioningTaskType taskType, string? assignedTo, JsonDocument? config)
    {
        if (IsRouterTask(taskType))
            return ResolveVendorAdapter(assignedTo, config);

        return taskType switch
        {
            ProvisioningTaskType.FtthOntProvision or ProvisioningTaskType.FtthServicePortConfig
                or ProvisioningTaskType.FtthVlanConfig or ProvisioningTaskType.FtthPppoeConfig
                or ProvisioningTaskType.AdslDslPortConfig or ProvisioningTaskType.AdslLineProfileConfig
                or ProvisioningTaskType.AdslAccessCredentials or ProvisioningTaskType.LteSubscriberActivation
                or ProvisioningTaskType.LteApnConfig or ProvisioningTaskType.LtePolicyProfile
                or ProvisioningTaskType.DiaCircuitConfig or ProvisioningTaskType.EthernetCircuitConfig
                or ProvisioningTaskType.StaticIpAllocation or ProvisioningTaskType.NetworkConfig
                or ProvisioningTaskType.WirelessTransmissionConfig or ProvisioningTaskType.AtmConnectivitySetup
                or ProvisioningTaskType.PriTrunkConfig or ProvisioningTaskType.TdmCircuitConfig
                or ProvisioningTaskType.WifiAccessConfig => "HUAWEI_BROADBAND",

            ProvisioningTaskType.NumberReservation or ProvisioningTaskType.NumberActivation
                or ProvisioningTaskType.SubscriberProfileConfig or ProvisioningTaskType.TelephonyFeatureConfig
                or ProvisioningTaskType.FreePhoneRouting => "ZTE_SOFTSWITCH",

            ProvisioningTaskType.DedicatedServerAllocate or ProvisioningTaskType.VpsAllocate
                or ProvisioningTaskType.ColocationRackAllocate or ProvisioningTaskType.WebHostingPlanSetup
                or ProvisioningTaskType.DomainRegistration or ProvisioningTaskType.DNSSetup
                or ProvisioningTaskType.AccountSetup or ProvisioningTaskType.EmailNotification => "INTERNAL",

            ProvisioningTaskType.PhysicalInstall => "FIELD_TECH",

            ProvisioningTaskType.ResourceAllocation or ProvisioningTaskType.QualificationCheck
                or ProvisioningTaskType.InventoryReservation or ProvisioningTaskType.Custom => "INTERNAL",

            _ => "INTERNAL"
        };
    }

    private static bool IsRouterTask(ProvisioningTaskType taskType)
        => taskType is ProvisioningTaskType.RouterInterfaceConfig
            or ProvisioningTaskType.RouterBgpConfig
            or ProvisioningTaskType.RouterOspfConfig
            or ProvisioningTaskType.RouterStaticRouteConfig
            or ProvisioningTaskType.RouterSystemConfig
            or ProvisioningTaskType.RouterAclConfig
            or ProvisioningTaskType.GetRouterStatus
            or ProvisioningTaskType.GetRouterInventory
            or ProvisioningTaskType.GetRouterAlarms;

    private static string ResolveVendorAdapter(string? assignedTo, JsonDocument? config)
    {
        if (config?.RootElement.TryGetProperty("vendor", out var vendorProp) == true)
        {
            var vendor = vendorProp.GetString();
            if (string.Equals(vendor, "juniper", StringComparison.OrdinalIgnoreCase))
                return "JUNIPER_ROUTER";
            if (string.Equals(vendor, "nokia", StringComparison.OrdinalIgnoreCase))
                return "NOKIA_ROUTER";
        }

        if (!string.IsNullOrEmpty(assignedTo))
        {
            var lower = assignedTo.ToLowerInvariant();
            if (lower.Contains("juniper")) return "JUNIPER_ROUTER";
            if (lower.Contains("nokia")) return "NOKIA_ROUTER";
        }

        return "CISCO_ROUTER";
    }

    private static void UpdateProgress(Guid jobId, int completed, int total, string status, string? currentTask, string? error)
    {
        ProgressCache[jobId] = new CoordinatorProgress(jobId, completed, total, status, currentTask, error);
    }

    private static CoordinatorProgress FailResult(Guid jobId, int completed, int total, string status, string? error = null)
    {
        var result = new CoordinatorProgress(jobId, completed, total, status, null, error);
        ProgressCache[jobId] = result;
        return result;
    }
}
