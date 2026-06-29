using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Obss.SharedKernel.Application.Abstractions;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Domain.ValueObjects;

namespace Obss.Workflow.Application.BackgroundJobs;

public sealed class WorkflowSlaMonitorJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WorkflowSlaMonitorJob> _logger;

    public WorkflowSlaMonitorJob(IServiceProvider serviceProvider, ILogger<WorkflowSlaMonitorJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Workflow SLA monitor job started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var instanceRepository = scope.ServiceProvider.GetRequiredService<IWorkflowInstanceRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var allInstances = await instanceRepository.GetAllAsync(stoppingToken);
                var runningInstances = allInstances
                    .Where(i => i.Status == InstanceStatus.Running && i.SlaDeadline.HasValue && !i.IsSlaBreached)
                    .ToList();

                foreach (var instance in runningInstances)
                {
                    instance.CheckSlaBreach();
                }

                await unitOfWork.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in workflow SLA monitor job.");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
