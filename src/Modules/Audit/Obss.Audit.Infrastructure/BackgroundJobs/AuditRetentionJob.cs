using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Obss.Audit.Application.Commands.PurgeOldEntries;

namespace Obss.Audit.Infrastructure.BackgroundJobs;

public sealed class AuditRetentionJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditRetentionJob> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);

    public AuditRetentionJob(IServiceScopeFactory scopeFactory, ILogger<AuditRetentionJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Audit retention job started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_interval, stoppingToken);

                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var result = await mediator.Send(new PurgeOldEntriesCommand(), stoppingToken);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Audit retention job purged {Count} entries.", result.Value);
                }
                else
                {
                    _logger.LogError("Audit retention job failed: {Error}", result.Error);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during audit retention job execution.");
            }
        }

        _logger.LogInformation("Audit retention job stopped.");
    }
}
