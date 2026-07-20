using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Application.Commands.GenerateBill;
using Obss.Billing.Application.Configuration;
using Obss.Billing.Domain.Entities;

namespace Obss.Billing.Application.BackgroundJobs;

public sealed class BillingGenerationJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BillingGenerationJob> _logger;
    private readonly IOptions<BillingConfiguration> _billingConfig;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);

    public BillingGenerationJob(
        IServiceProvider serviceProvider,
        ILogger<BillingGenerationJob> logger,
        IOptions<BillingConfiguration> billingConfig)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _billingConfig = billingConfig;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Billing generation job started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBillingCycles(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error occurred while processing billing cycles.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task ProcessBillingCycles(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var billingCycleRepository = scope.ServiceProvider.GetRequiredService<IBillingCycleRepository>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var now = DateTime.UtcNow;
        var cyclesDue = await billingCycleRepository.GetCyclesDueAsync(now, cancellationToken);

        _logger.LogInformation("Found {Count} billing cycles due for processing.", cyclesDue.Count);

        foreach (var cycle in cyclesDue)
        {
            try
            {
                var periodStart = cycle.LastBillingDate;
                var periodEnd = cycle.NextBillingDate;
                var dueDate = periodEnd.AddDays(15);

                var result = await mediator.Send(
                    new GenerateBillCommand(
                        cycle.CustomerId,
                        string.Empty,
                        cycle.BillingPeriod.ToString(),
                        periodStart,
                        periodEnd,
                        dueDate,
                        _billingConfig.Value.DefaultCurrency),
                    cancellationToken);

                if (result.IsSuccess)
                {
                    cycle.AdvanceToNextCycle();
                    _logger.LogInformation(
                        "Bill generated for customer {CustomerId} from cycle {CycleId}.",
                        cycle.CustomerId,
                        cycle.Id);
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to generate bill for customer {CustomerId}: {Error}",
                        cycle.CustomerId,
                        result.Error.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error generating bill for billing cycle {CycleId}.",
                    cycle.Id);
            }
        }
    }
}
