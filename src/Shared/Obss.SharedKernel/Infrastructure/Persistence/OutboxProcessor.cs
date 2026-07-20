using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Obss.SharedKernel.Infrastructure.Diagnostics;
using Obss.SharedKernel.Infrastructure.EventBus;
using Obss.SharedKernel.Infrastructure.Serialization;
using RabbitMQ.Client;

namespace Obss.SharedKernel.Infrastructure.Persistence;

public sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<RabbitMqConfiguration> _rabbitMqConfiguration;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly OutboxMetrics _metrics;
    private readonly TimeSpan _pollingInterval;
    private readonly TimeSpan _lockDuration;

    public OutboxProcessor(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqConfiguration> rabbitMqConfiguration,
        ILogger<OutboxProcessor> logger,
        OutboxMetrics metrics,
        TimeSpan? pollingInterval = null)
    {
        _scopeFactory = scopeFactory;
        _rabbitMqConfiguration = rabbitMqConfiguration;
        _logger = logger;
        _metrics = metrics;
        _pollingInterval = pollingInterval ?? TimeSpan.FromSeconds(10);
        _lockDuration = TimeSpan.FromMinutes(2);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxProcessor started with polling interval {PollingInterval}", _pollingInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("OutboxProcessor stopped");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var contexts = scope.ServiceProvider.GetServices<EfDbContext>();
        var instanceId = Guid.NewGuid();

        var connection = await CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(
            cancellationToken: cancellationToken);

        var dlxExchangeName = "obss_integration_events.dlx";

        await channel.ExchangeDeclareAsync(
            exchange: dlxExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        foreach (var context in contexts)
        {
            var now = DateTime.UtcNow;

            var messages = await context.OutboxMessages
                .Where(m => m.ProcessedAt == null)
                .Where(m => !m.IsDeadLettered)
                .Where(m => m.NextAttemptAt == null || m.NextAttemptAt <= now)
                .Where(m => m.LockExpiresAt == null || m.LockExpiresAt <= now)
                .OrderBy(m => m.CreatedAt)
                .Take(50)
                .ToListAsync(cancellationToken);

            if (messages.Count == 0)
                continue;

            foreach (var message in messages)
            {
                if (!message.TryAcquireLock(instanceId, _lockDuration))
                    continue;

                var startedAt = DateTime.UtcNow;

                try
                {
                    await PublishMessageAsync(channel, message, cancellationToken);

                    message.MarkAsProcessed();
                    context.OutboxMessages.Update(message);

                    _metrics.MessageProcessed();
                    _metrics.RecordProcessingDuration((DateTime.UtcNow - startedAt).TotalMilliseconds);

                    _logger.LogInformation(
                        "Published event {MessageId} of type {EventType}",
                        message.Id, message.EventType);
                }
                catch (Exception ex)
                {
                    _metrics.MessageFailed();

                    _logger.LogError(
                        ex,
                        "Failed to process outbox message {MessageId} of type {EventType}",
                        message.Id, message.EventType);

                    if (message.CanRetry())
                    {
                        _metrics.RecordProcessingDuration((DateTime.UtcNow - startedAt).TotalMilliseconds);
                        message.RecordFailure(ex.Message);
                        message.ReleaseLock();
                    }
                    else
                    {
                        _metrics.MessageDeadLettered();
                        _metrics.RecordProcessingDuration((DateTime.UtcNow - startedAt).TotalMilliseconds);

                        message.MarkAsDeadLettered(
                            $"Max retries ({message.RetryCount}) exceeded or non-retryable: {ex.Message}");

                        _logger.LogWarning(
                            "Outbox message {MessageId} of type {EventType} moved to dead-letter after {RetryCount} retries",
                            message.Id, message.EventType, message.RetryCount);
                    }

                    context.OutboxMessages.Update(message);
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task PublishMessageAsync(
        IChannel channel,
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        var exchangeName = $"integration.{message.EventType}";

        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            MessageId = message.Id.ToString(),
            Timestamp = new AmqpTimestamp(
                DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        if (!string.IsNullOrEmpty(message.CorrelationId))
        {
            properties.CorrelationId = message.CorrelationId;
        }

        if (message.RetryCount > 0)
        {
            properties.Headers = new Dictionary<string, object?>
            {
                { "x-retry-count", Encoding.UTF8.GetBytes(message.RetryCount.ToString()) }
            };
        }

        await channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: message.EventType,
            mandatory: true,
            basicProperties: properties,
            body: Encoding.UTF8.GetBytes(message.EventData),
            cancellationToken: cancellationToken);
    }

    private async Task<IConnection> CreateConnectionAsync(CancellationToken cancellationToken)
    {
        var config = _rabbitMqConfiguration.Value;

        var factory = new ConnectionFactory
        {
            HostName = config.Host,
            Port = config.Port,
            UserName = config.Username,
            Password = config.Password,
            VirtualHost = config.VirtualHost,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        return await factory.CreateConnectionAsync(cancellationToken);
    }
}
