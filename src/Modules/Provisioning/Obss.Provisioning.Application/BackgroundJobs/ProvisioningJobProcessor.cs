using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Application.Diagnostics;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Provisioning.Application.BackgroundJobs;

public sealed class ProvisioningJobProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProvisioningJobProcessor> _logger;
    private readonly ProvisioningMetrics _metrics;

    public ProvisioningJobProcessor(IServiceProvider serviceProvider, ILogger<ProvisioningJobProcessor> logger, ProvisioningMetrics metrics)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _metrics = metrics;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Provisioning job processor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var jobRepository = scope.ServiceProvider.GetRequiredService<IProvisioningJobRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var adapters = scope.ServiceProvider.GetServices<IProvisioningAdapter>();

                var queuedJobs = await jobRepository.GetQueuedJobsAsync(stoppingToken);

                foreach (var job in queuedJobs)
                {
                    try
                    {
                        job.Start();

                        foreach (var task in job.Tasks.Where(t => t.Status == ProvisioningTaskStatus.Pending))
                        {
                            task.Start();
                            _metrics.TaskStarted();

                            var adapter = ResolveAdapter(adapters, task);

                            if (adapter is null)
                            {
                                task.Fail($"No adapter available for task '{task.TaskType}' (assigned to '{task.AssignedTo}')");
                                _metrics.TaskFailed();
                                continue;
                            }

                            using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                            cts.CancelAfter(TimeSpan.FromMinutes(5));

                            var startedAt = DateTime.UtcNow;
                            var result = await adapter.ExecuteAsync(task, cts.Token);
                            var elapsed = (DateTime.UtcNow - startedAt).TotalMilliseconds;

                            if (result.Success)
                            {
                                if (result.ResultData is not null)
                                {
                                    task.Complete(JsonSerializer.Deserialize<JsonDocument>(result.ResultData));
                                }
                                else
                                {
                                    task.Complete();
                                }

                                _metrics.TaskCompleted();
                                _metrics.RecordTaskDuration(elapsed);

                                _logger.LogInformation(
                                    "Task {TaskId} ({TaskType}) completed via adapter {Adapter}",
                                    task.Id, task.TaskType, adapter.AdapterName);
                            }
                            else
                            {
                                task.Fail(result.ErrorMessage ?? "Unknown error");
                                _metrics.TaskFailed();
                                _metrics.RecordTaskDuration(elapsed);

                                _logger.LogWarning(
                                    "Task {TaskId} ({TaskType}) failed via adapter {Adapter}: {Error}",
                                    task.Id, task.TaskType, adapter.AdapterName, result.ErrorMessage);
                            }
                        }

                        if (job.Tasks.All(t => t.Status == ProvisioningTaskStatus.Completed))
                        {
                            job.Complete();
                        }
                        else
                        {
                            job.Fail("One or more tasks failed");
                        }

                        await unitOfWork.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation(
                            job.Tasks.All(t => t.Status == ProvisioningTaskStatus.Completed)
                                ? "Provisioning job {JobId} completed successfully."
                                : "Provisioning job {JobId} completed with failures.",
                            job.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing provisioning job {JobId}", job.Id);
                        job.Fail(ex.Message);
                        await unitOfWork.SaveChangesAsync(stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in provisioning job processor.");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private static IProvisioningAdapter? ResolveAdapter(IEnumerable<IProvisioningAdapter> adapters, ProvisioningTask task)
    {
        var adapterList = adapters.ToList();

        if (!string.IsNullOrWhiteSpace(task.AssignedTo))
        {
            var named = adapterList.FirstOrDefault(a =>
                a.AdapterName.Equals(task.AssignedTo, StringComparison.OrdinalIgnoreCase));
            if (named is not null)
                return named;
        }

        var supportedTypes = task.TaskType switch
        {
            ProvisioningTaskType.NetworkConfig or ProvisioningTaskType.ResourceAllocation => "NetworkAdapter",
            ProvisioningTaskType.DNSSetup => "DnsAdapter",
            ProvisioningTaskType.AccountSetup or ProvisioningTaskType.EmailNotification => "AccountAdapter",
            _ => "TestAdapter"
        };

        return adapterList.FirstOrDefault(a =>
            a.AdapterName.Equals(supportedTypes, StringComparison.OrdinalIgnoreCase))
            ?? adapterList.FirstOrDefault(a => a.AdapterName == "TestAdapter");
    }
}
