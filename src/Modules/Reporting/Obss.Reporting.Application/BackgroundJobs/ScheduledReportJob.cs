using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Obss.Reporting.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Reporting.Application.BackgroundJobs;

public sealed class ScheduledReportJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScheduledReportJob> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public ScheduledReportJob(IServiceProvider serviceProvider, ILogger<ScheduledReportJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Scheduled report job started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessScheduledReports(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error occurred while processing scheduled reports.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task ProcessScheduledReports(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var reportRepository = scope.ServiceProvider.GetRequiredService<IReportRepository>();
        var reportGenerator = scope.ServiceProvider.GetRequiredService<IReportGenerator>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var now = DateTime.UtcNow;
        var dueReports = await reportRepository.GetScheduledReportsDueAsync(now, cancellationToken);

        foreach (var scheduledReport in dueReports)
        {
            try
            {
                var reportDefinition = await reportRepository.GetByIdAsync<Guid>(scheduledReport.ReportDefinitionId, cancellationToken);
                if (reportDefinition is null || !reportDefinition.IsActive)
                {
                    scheduledReport.Deactivate();
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                    continue;
                }

                var execution = new Obss.Reporting.Domain.Entities.ReportExecution(
                    Guid.NewGuid(),
                    scheduledReport.ReportDefinitionId,
                    "system");

                execution.Start();
                await reportRepository.AddExecutionAsync(execution, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                var (filePath, fileSize) = await reportGenerator.GenerateAsync(reportDefinition, cancellationToken);
                execution.Complete(filePath, fileSize);

                scheduledReport.MarkRan();

                _logger.LogInformation(
                    "Scheduled report {ScheduledReportId} executed successfully.",
                    scheduledReport.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error executing scheduled report {ScheduledReportId}.",
                    scheduledReport.Id);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
