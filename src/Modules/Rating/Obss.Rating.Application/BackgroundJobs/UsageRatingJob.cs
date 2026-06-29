using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Obss.Rating.Application.Commands.RateUsage;

namespace Obss.Rating.Application.BackgroundJobs;

public sealed class UsageRatingJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UsageRatingJob> _logger;
    private readonly TimeSpan _pollingInterval;

    public UsageRatingJob(IServiceProvider serviceProvider, ILogger<UsageRatingJob> logger)
        : this(serviceProvider, logger, TimeSpan.FromSeconds(30))
    {
    }

    public UsageRatingJob(IServiceProvider serviceProvider, ILogger<UsageRatingJob> logger, TimeSpan pollingInterval)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _pollingInterval = pollingInterval;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Usage rating job started with polling interval {Interval}", _pollingInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var result = await mediator.Send(new RateUsageCommand(), stoppingToken);

                if (result.IsSuccess && result.Value > 0)
                {
                    _logger.LogInformation("Usage rating job rated {Count} records", result.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Usage rating job encountered an error");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("Usage rating job stopped");
    }
}
