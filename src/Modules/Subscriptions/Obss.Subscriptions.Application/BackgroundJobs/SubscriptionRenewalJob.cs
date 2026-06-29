using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Application.Commands.RenewSubscription;

namespace Obss.Subscriptions.Application.BackgroundJobs;

public sealed class SubscriptionRenewalJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SubscriptionRenewalJob> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);

    public SubscriptionRenewalJob(
        IServiceProvider serviceProvider,
        ILogger<SubscriptionRenewalJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Subscription renewal job started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRenewals(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error occurred while processing subscription renewals.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task ProcessRenewals(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var subscriptionRepository = scope.ServiceProvider.GetRequiredService<ISubscriptionRepository>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var now = DateTime.UtcNow;
        var subscriptionsDue = await subscriptionRepository.GetSubscriptionsDueForRenewalAsync(now, cancellationToken);

        _logger.LogInformation("Found {Count} subscriptions due for renewal.", subscriptionsDue.Count);

        foreach (var subscriptionId in subscriptionsDue.Select(s => s.Id))
        {
            try
            {
                var result = await mediator.Send(
                    new RenewSubscriptionCommand(subscriptionId),
                    cancellationToken);

                if (result.IsSuccess)
                {
                    _logger.LogInformation(
                        "Successfully renewed subscription {SubscriptionId}.",
                        subscriptionId);
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to renew subscription {SubscriptionId}: {Error}",
                        subscriptionId,
                        result.Error.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error renewing subscription {SubscriptionId}.",
                    subscriptionId);
            }
        }
    }
}
