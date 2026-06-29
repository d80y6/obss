using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Domain.ValueObjects;

namespace Obss.Orders.Application.BackgroundJobs;

public sealed class OrderFulfillmentJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderFulfillmentJob> _logger;
    private readonly TimeSpan _pollingInterval;

    public OrderFulfillmentJob(
        IServiceScopeFactory scopeFactory,
        ILogger<OrderFulfillmentJob> logger,
        TimeSpan? pollingInterval = null)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _pollingInterval = pollingInterval ?? TimeSpan.FromSeconds(30);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OrderFulfillmentJob started with polling interval {PollingInterval}", _pollingInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingFulfillmentsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error processing pending order fulfillments");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("OrderFulfillmentJob stopped");
    }

    private async Task ProcessPendingFulfillmentsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOrderFulfillmentRepository>();

        var pendingFulfillments = await repository.GetByStatusAsync(FulfillmentStatus.Pending, cancellationToken);

        foreach (var fulfillment in pendingFulfillments)
        {
            try
            {
                fulfillment.StartFulfillment(Guid.NewGuid());
                await repository.UpdateAsync(fulfillment, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to start fulfillment {FulfillmentId} for order {OrderId}",
                    fulfillment.Id,
                    fulfillment.OrderId);
            }
        }
    }
}
