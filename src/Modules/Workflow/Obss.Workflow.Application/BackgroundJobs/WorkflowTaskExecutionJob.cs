using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Obss.SharedKernel.Application.Abstractions;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Domain.Services;

namespace Obss.Workflow.Application.BackgroundJobs;

public sealed class WorkflowTaskExecutionJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WorkflowTaskExecutionJob> _logger;

    public WorkflowTaskExecutionJob(IServiceProvider serviceProvider, ILogger<WorkflowTaskExecutionJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Workflow task execution job started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var instanceRepository = scope.ServiceProvider.GetRequiredService<IWorkflowInstanceRepository>();
                var workflowEngine = scope.ServiceProvider.GetRequiredService<IWorkflowEngine>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var pendingTasks = await instanceRepository.GetPendingTasksAsync(stoppingToken);

                foreach (var task in pendingTasks)
                {
                    try
                    {
                        await workflowEngine.ExecuteStep(task.WorkflowInstanceId, task.Id, stoppingToken);
                        await unitOfWork.SaveChangesAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing workflow task {TaskId}", task.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in workflow task execution job.");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
