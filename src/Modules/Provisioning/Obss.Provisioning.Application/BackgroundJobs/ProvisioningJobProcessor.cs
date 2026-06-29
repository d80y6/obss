using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Provisioning.Application.BackgroundJobs;

public sealed class ProvisioningJobProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProvisioningJobProcessor> _logger;

    public ProvisioningJobProcessor(IServiceProvider serviceProvider, ILogger<ProvisioningJobProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
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

                var queuedJobs = await jobRepository.GetQueuedJobsAsync(stoppingToken);

                foreach (var job in queuedJobs)
                {
                    try
                    {
                        job.Start();

                        foreach (var task in job.Tasks.Where(t => t.Status == Domain.ValueObjects.ProvisioningTaskStatus.Pending))
                        {
                            task.Start();

                            // Simulate task execution - in production, delegate to workflow engine
                            await Task.Delay(100, stoppingToken);

                            task.Complete();
                        }

                        job.Complete();
                        await unitOfWork.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation("Provisioning job {JobId} completed successfully.", job.Id);
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
}
